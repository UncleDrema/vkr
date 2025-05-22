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
                .Without<GraphComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var graph in _initRequests)
            {
                ref var cInitRequest = ref graph.GetComponent<InitializeGraphRequest>();
                
                var segmentLength = cInitRequest.DesiredSegmentLength;
                var nodes = cInitRequest.Nodes;
                var edges = cInitRequest.Edges;

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
                cGraph.VertexDistances = new Dictionary<Entity, Dictionary<Entity, float>>();
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

                ComputeDistances(ref cGraph);
                
                allPositions.Dispose();
                edgeIndices.Dispose();
                mapOldToEntity.Dispose();
            }
        }

        // Рассчет расстояний между вершинами графа алгоритмом Дейкстры
        private void ComputeDistances(ref GraphComponent cGraph)
        {
            var vertices = cGraph.Vertices;
            var edges = cGraph.Edges;
            var vertexDistances = cGraph.VertexDistances;
            
            foreach (var vertex in vertices)
            {
                var distances = new Dictionary<Entity, float>();
                var queue = new List<Entity>();
                var visited = new HashSet<Entity>();

                distances[vertex] = 0f;
                queue.Add(vertex);

                while (queue.Count > 0)
                {
                    Entity current = queue[0];
                    queue.RemoveAt(0);
                    visited.Add(current);

                    ref var cCurrent = ref current.GetComponent<GraphVertexComponent>();
                    foreach (var neighbor in cCurrent.Neighbors)
                    {
                        if (visited.Contains(neighbor))
                            continue;

                        float distance = math.distance(cCurrent.Position, neighbor.GetComponent<GraphVertexComponent>().Position);
                        if (!distances.ContainsKey(neighbor) || distances[neighbor] > distances[current] + distance)
                        {
                            distances[neighbor] = distances[current] + distance;
                            queue.Add(neighbor);
                        }
                    }
                }

                vertexDistances[vertex] = distances;
            }
        }
    }
}