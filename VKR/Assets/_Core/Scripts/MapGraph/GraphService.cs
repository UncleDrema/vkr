using Game.MapGraph.Components;
using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEngine;

namespace Game.MapGraph
{
    public class GraphService
    {
        private World _world;
        private Filter _graphFilter;

        public GraphService()
        {
            Debug.Log($"GraphService created: {this}");
        }

        public void Initialize(World world)
        {
            Debug.Log($"GraphService initialized: {this}");
            _world = world;
            _graphFilter = world.Filter.With<GraphComponent>().Build();
        }
        
        public Entity GetNearestVertex(float3 position)
        {
            float minDistance = float.MaxValue;
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