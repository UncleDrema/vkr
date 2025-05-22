using System.Collections.Generic;
using Game.MapGraph.Components;
using Game.MapGraph.Events;
using Game.MapGraph.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Feature.Events;
using Scellecs.Morpeh.Addons.Systems;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.MapGraph.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class InitializeGraphZonesSystem : UpdateSystem
    {
        private Filter _initRequests;
        private Filter _graphs;
        private Filter _zones;
        
        public override void OnAwake()
        {
            _initRequests = World.Filter
                .With<InitializeGraphZonesRequest>()
                .Build();
            _graphs = World.Filter
                .With<GraphComponent>()
                .Build();
            _zones = World.Filter
                .With<ZoneComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var request in _initRequests)
            {
                ref var cRequest = ref request.GetComponent<InitializeGraphZonesRequest>();
                var zoneCount = cRequest.ZoneCount;
                
                foreach (var graph in _graphs)
                {
                    ref var cGraph = ref graph.GetComponent<GraphComponent>();
                    ClearZones(ref cGraph);
                    InitializeZones(ref cGraph, zoneCount);
                }
            }
        }

        private void ClearZones(ref GraphComponent cGraph)
        {
            foreach (var zone in _zones)
            {
                zone.RemoveComponent<ZoneComponent>();
            }
            
            foreach (var vertex in cGraph.Vertices)
            {
                ref var cVertex = ref vertex.GetComponent<GraphVertexComponent>();
                cVertex.Zone = default;
            }
        }

        private void InitializeZones(ref GraphComponent cGraph, int zoneCount)
        {
            float3 center = new float3(0, 0, 0);
            // alpha
            float distanceFromCenterSignificance = 1f;
            // beta
            float distanceFromOtherSignificance = 3f;
            
            var vertices = cGraph.Vertices;
            var n = vertices.Count;
            var m = zoneCount;

            var seeds = new NativeArray<Entity>(m, Allocator.Temp);
            var seedPositions = new NativeArray<float3>(m, Allocator.Temp);
            var vertexPositions = new NativeArray<float3>(n, Allocator.Temp);
            for (var i = 0; i < vertices.Count; i++)
                vertexPositions[i] = vertices[i].GetComponent<GraphVertexComponent>().Position;
            
            for (int k = 0; k < m; k++)
            {
                float bestScore = float.NegativeInfinity;
                int bestIdx = -1;

                for (int i = 0; i < n; i++)
                {
                    var v = vertices[i];
                    if (seeds.Contains(v))
                        continue;
                    
                    float3 p = vertexPositions[i];
                    float distanceFromCenter = math.distance(p, center);

                    float distanceFromSeeds = 0;
                    for (int j = 0; j < k; j++)
                    {
                        var sp = seedPositions[j];
                        distanceFromSeeds += math.distance(p, sp);
                    }
                    
                    float score = distanceFromOtherSignificance * distanceFromSeeds - distanceFromCenterSignificance * distanceFromCenter;
                    score *= Random.Range(0.7f, 1.3f);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIdx = i;
                    }
                }
                
                seeds[k] = vertices[bestIdx];
                seedPositions[k] = vertexPositions[bestIdx];
            }
            
            // Назначаем зоны, начинающиеся в сидах
            NativeList<Entity> zones = new NativeList<Entity>(Allocator.Temp);
            for (int k = 0; k < m; k++)
            {
                var zone = World.CreateEntity();
                ref var cZone = ref zone.AddComponent<ZoneComponent>();
                cZone.ZoneId = k;
                cZone.Vertices = new List<Entity>();
                var seed = seeds[k];
                ref var cSeed = ref seed.GetComponent<GraphVertexComponent>();
                cSeed.Zone = zone;
                cZone.Vertices.Add(seed);
                zones.Add(zone);
            }
            
            var boundary = new NativeList<Entity>(Allocator.Temp);
            bool growing = true;
            int iteration = 0;
            while (growing)
            {
                growing = false;
                zones.Shuffle();
                foreach (var zone in zones)
                {
                    ref var cZone = ref zone.GetComponent<ZoneComponent>();
                    // Соберем граничные вершины
                    boundary.Clear();
                    foreach (var v in cZone.Vertices)
                    {
                        ref var cVertex = ref v.GetComponent<GraphVertexComponent>();
                        var neighbors = cVertex.Neighbors;
                        foreach (var neighbor in neighbors)
                        {
                            ref var cNeighbor = ref neighbor.GetComponent<GraphVertexComponent>();
                            if (cNeighbor.Zone == default)
                            {
                                boundary.Add(neighbor);
                                cNeighbor.Zone = zone;
                            }
                        }
                    }
                    if (boundary.Length == 0)
                        continue;
                    
                    foreach (var v in boundary)
                    {
                        cZone.Vertices.Add(v);
                        growing = true;
                    }
                }
                
                iteration++;
                if (iteration > 1000)
                {
                    Debug.LogError("Infinite loop in InitializeGraphZonesSystem");
                    break;
                }
            }

            boundary.Dispose();
            seeds.Dispose();
            seedPositions.Dispose();
            vertexPositions.Dispose();

            World.CreateEventEntity<ZonesInitializedEvent>();
        }
    }
}