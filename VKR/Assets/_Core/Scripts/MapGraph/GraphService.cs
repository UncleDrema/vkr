using System.Collections.Generic;
using Game.MapGraph.Components;
using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEngine;

namespace Game.MapGraph
{
    public class GraphService
    {
        private Filter _graphFilter;

        public void Initialize(World world)
        {
            _graphFilter = world.Filter.With<GraphComponent>().Build();
        }

        public float GetDistance(Entity from, Entity to)
        {
            foreach (var entity in _graphFilter)
            {
                ref var cGraph = ref entity.GetComponent<GraphComponent>();
                
                if (cGraph.VertexDistances.TryGetValue(from, out var distances) && 
                    distances.TryGetValue(to, out var distance))
                {
                    return distance;
                }
            }
            
            Debug.LogError($"GraphService: No distance found between {from} and {to}");
            return float.MaxValue;
        }
        
        public Entity GetNearestVertex(float3 position, out float minDistance)
        {
            minDistance = float.MaxValue;
            Entity nearestVertex = default;
            
            foreach (var entity in _graphFilter)
            {
                ref var cGraph = ref entity.GetComponent<GraphComponent>();
                
                foreach (var vertex in cGraph.Vertices)
                {
                    ref var vertexComponent = ref vertex.GetComponent<GraphVertexComponent>();
                    float distance = math.distancesq(vertexComponent.Position, position);
                    
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestVertex = vertex;
                    }
                }
            }
            
            return nearestVertex;
        }

        public List<Entity> GetPath(Entity nearestVertex, Entity oldestVertex)
        {
            // Ищем путь в графе через BFS
            foreach (var entity in _graphFilter)
            {
                ref var cGraph = ref entity.GetComponent<GraphComponent>();
                
                var queue = new Queue<Entity>();
                var visited = new HashSet<Entity>();
                var cameFrom = new Dictionary<Entity, Entity>();
                
                queue.Enqueue(nearestVertex);
                visited.Add(nearestVertex);

                bool found = false;

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();

                    if (current.Equals(oldestVertex))
                    {
                        found = true;
                        break;
                    }

                    ref var currentVertex = ref current.GetComponent<GraphVertexComponent>();
                    foreach (var neighbor in currentVertex.Neighbors)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                            cameFrom[neighbor] = current;
                        }
                    }
                }

                if (!found)
                {
                    Debug.LogError($"GraphService: No path found from {nearestVertex} to {oldestVertex}");
                    return new List<Entity>();
                }

                // Восстановление пути
                var path = new List<Entity>();
                var step = oldestVertex;
                while (!step.Equals(nearestVertex))
                {
                    path.Add(step);
                    step = cameFrom[step];
                }
                path.Add(nearestVertex);
                path.Reverse();
                return path;
            }
            
            Debug.LogError($"GraphService: No path found from {nearestVertex} to {oldestVertex}");
            return new List<Entity>();
        }
    }
}