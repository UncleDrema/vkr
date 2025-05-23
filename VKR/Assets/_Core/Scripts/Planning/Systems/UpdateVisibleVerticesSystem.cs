using Game.MapGraph;
using Game.MapGraph.Components;
using Game.PotentialField.Components;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Transform.Components;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Planning.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UpdateVisibleVerticesSystem : UpdateSystem
    {
        private readonly GraphService _graphService;
        
        private Filter _agents;
        private Filter _vertices;
        private Filter _obstacles;
        
        public UpdateVisibleVerticesSystem(GraphService graphService)
        {
            _graphService = graphService;
        }
        
        public override void OnAwake()
        {
            _agents = World.Filter
                .With<AgentLocalFieldComponent>()
                .With<TransformComponent>()
                .Build();
            _vertices = World.Filter
                .With<GraphVertexComponent>()
                .Build();
            _obstacles = World.Filter
                .With<ObstacleComponent>()
                .With<TransformComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            float now = Time.time;
            foreach (var agent in _agents)
            {
                ref var cTransform = ref agent.GetComponent<TransformComponent>();
                var position = cTransform.Position();
                var rotation = cTransform.Rotation();
                var agentRadius = 3f;
                var smallAgentRadius = 0.4f;
                var agentAperture = 60f;
                
                // Рассмотрим все вершины в секторе, который видит агент с радиусом agentRadius и углом agentAperture
                foreach (var vertex in _vertices)
                {
                    ref var cVertex = ref vertex.GetComponent<GraphVertexComponent>();
                    var vertexPosition = cVertex.Position;
                    
                    if (PlanningUtils.PointInRadius(position, smallAgentRadius, vertexPosition) ||
                        (PlanningUtils.PointInSector(position, rotation, agentRadius, agentAperture, vertexPosition) &&
                         IsVisible(position, vertexPosition)))
                    {
                        cVertex.LastObservationTime = now;
                    }
                }
            }
        }

        private bool IsVisible(float3 position, float3 vertexPosition)
        {
            bool isVisible = true;
            foreach (var obstacle in _obstacles)
            {
                var bounds = obstacle.GetComponent<ObstacleComponent>().Bounds;
                // Проверяем, пересекается ли прямая от агента до вершины с препятствием
                if (PlanningUtils.LineIntersectsBounds(position, vertexPosition, bounds))
                {
                    isVisible = false;
                    break;
                }
            }
            
            return isVisible;
        }
    }
}