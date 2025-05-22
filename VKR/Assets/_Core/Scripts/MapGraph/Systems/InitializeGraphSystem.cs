using System;
using System.Collections.Generic;
using Game.MapGraph.Components;
using Game.MapGraph.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Game.MapGraph.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class InitializeGraphSystem : UpdateSystem
    {
        private Filter _initRequests;
        
        public override void OnAwake()
        {
            _initRequests = World.Filter
                .With<InitializeGraphRequest>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var graph in _initRequests)
            {
                ref var cInitRequest = ref graph.GetComponent<InitializeGraphRequest>();
                
                var segmentLength = cInitRequest.DesiredSegmentLength;
                var levelGraph = cInitRequest.LevelGraphData;
                var nodes = levelGraph.nodes;
                var edges = levelGraph.edges;

                var allPositions = new NativeList<float3>(nodes.Count, Allocator.Temp);
                var edgeIndices = new NativeList<int2>(edges.Count, Allocator.Temp);

                for (int i = 0; i < nodes.Count; i++)
                {
                    allPositions.Add(nodes[i]);
                }

                foreach (var e in edges)
                {
                    float3 a = allPositions[e.a];
                    float3 b = allPositions[e.b];
                    float dist = math.distance(a, b);
                    int segments = math.max(1, (int)math.ceil(dist / segmentLength));
                    int startIndex = e.a;
                    
                    for (int s = 1; s <= segments; s++)
                    {
                        float t = (float)s / segments;
                        float3 p = math.lerp(a, b, t);
                        if (s < segments)
                        {
                            allPositions.Add(p);
                            int idx = allPositions.Length - 1;
                            edgeIndices.Add(new int2(startIndex, idx));
                            startIndex = idx;
                        }
                        else
                        {
                            edgeIndices.Add(new int2(startIndex, e.b));
                        }
                    }
                }
                
                ref var cGraph = ref graph.AddComponent<GraphComponent>();
                cGraph.Vertices = new List<Entity>(nodes.Count);
                cGraph.Edges = new List<int2>(edges.Count);
                var mapOldToEntity = new NativeArray<Entity>(allPositions.Length, Allocator.Temp);
                
                for (int i = 0; i < allPositions.Length; i++)
                {
                    var node = allPositions[i];
                    var vertex = World.CreateEntity();
                    ref var cVertex = ref vertex.AddComponent<GraphVertexComponent>();

                    cVertex.Position = node;
                    cVertex.Neighbors = new List<Entity>(2);
                    cVertex.Threat = 0f;
                    cVertex.LastObservationTime = Time.time;

                    cGraph.Vertices.Add(vertex);
                    mapOldToEntity[i] = vertex;
                }
                
                foreach (var idx in edgeIndices)
                {
                    var va = mapOldToEntity[idx.x];
                    var vb = mapOldToEntity[idx.y];
                    // добавить vb в соседей va
                    ref var na = ref va.GetComponent<GraphVertexComponent>();
                    na.Neighbors.Add(vb);
                    // и наоборот
                    ref var nb = ref vb.GetComponent<GraphVertexComponent>();
                    nb.Neighbors.Add(va);
                    
                    cGraph.Edges.Add(idx);
                }
                
                allPositions.Dispose();
                edgeIndices.Dispose();
                mapOldToEntity.Dispose();
            }
        }
    }
}