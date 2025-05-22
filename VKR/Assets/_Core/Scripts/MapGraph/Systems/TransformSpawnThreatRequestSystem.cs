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
    public sealed class TransformSpawnThreatRequestSystem : UpdateSystem
    {
        private Filter _spawnThreatByPositionRequests;
        
        private GraphService _graphService;
        
        public TransformSpawnThreatRequestSystem(GraphService graphService)
        {
            _graphService = graphService;
        }
        
        public override void OnAwake()
        {
            _spawnThreatByPositionRequests = World.Filter
                .With<SpawnThreatByPositionRequest>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var request in _spawnThreatByPositionRequests)
            {
                ref var cSpawnRequest = ref request.GetComponent<SpawnThreatByPositionRequest>();
                var spawnPosition = cSpawnRequest.Position;
                
                // Transform the position to the graph space
                var vertex = _graphService.GetNearestVertex(spawnPosition, out float distance);
                if (vertex == default)
                {
                    Debug.LogError($"No vertex found for position {spawnPosition}");
                }
                else
                {
                    ref var cVertex = ref vertex.GetComponent<GraphVertexComponent>();
                    var neighbors = cVertex.Neighbors;
                    
                    // Получим вершину и всех её соседей, а также их расстояния до цели
                    var vertexAndNeighbours = new NativeList<Entity>(neighbors.Count + 1, Allocator.Temp);
                    var vertexAndNeighboursDistances = new NativeList<float>(neighbors.Count + 1, Allocator.Temp);
                    vertexAndNeighbours.Add(vertex);
                    vertexAndNeighboursDistances.Add(distance);
                    
                    foreach (var neighbor in neighbors)
                    {
                        ref var cNeighbor = ref neighbor.GetComponent<GraphVertexComponent>();
                        distance = math.distance(cNeighbor.Position, spawnPosition);
                        vertexAndNeighbours.Add(neighbor);
                        vertexAndNeighboursDistances.Add(distance);
                    }
                    
                    // Получим обратные расстояния и их сумму
                    float totalInverseSum = 0f;
                    
                    for (int i = 0; i < vertexAndNeighboursDistances.Length; i++)
                    {
                        var inverseDistance = 1f / vertexAndNeighboursDistances[i];
                        vertexAndNeighboursDistances[i] = inverseDistance;
                        totalInverseSum += inverseDistance;
                    }
                    for (int i = 0; i < vertexAndNeighboursDistances.Length; i++)
                    {
                        vertexAndNeighboursDistances[i] /= totalInverseSum;
                    }
                    
                    // Для всех вершин создадим SpawnThreatRequest с уровнем угрозы пропорциональным расстоянию

                    for (int i = 0; i < vertexAndNeighbours.Length; i++)
                    {
                        var targetVertex = vertexAndNeighbours[i];
                        var proportionalThreatLevel = cSpawnRequest.ThreatLevel * vertexAndNeighboursDistances[i];
                        
                        var spawnThreatRequest = World.CreateEntity();
                        ref var cSpawnThreatRequest = ref spawnThreatRequest.AddComponent<SpawnThreatRequest>();
                        cSpawnThreatRequest.TargetVertex = targetVertex;
                        cSpawnThreatRequest.ThreatLevel = proportionalThreatLevel;
                        cSpawnThreatRequest.ThreadDuration = cSpawnRequest.ThreadDuration;
                        cSpawnThreatRequest.DecayType = cSpawnRequest.DecayType;
                    }
                }
            }
        }
    }
}