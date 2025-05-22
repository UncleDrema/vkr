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
                ref var graphComponent = ref entity.GetComponent<GraphComponent>();
                
                if (graphComponent.VertexDistances.TryGetValue(from, out var distances) && 
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
                ref var graphComponent = ref entity.GetComponent<GraphComponent>();
                
                foreach (var vertex in graphComponent.Vertices)
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
    }
}