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
                var agentAperture = 60f;
                
                // Рассмотрим все вершины в секторе, который видит агент с радиусом agentRadius и углом agentAperture
                foreach (var vertex in _vertices)
                {
                    ref var cVertex = ref vertex.GetComponent<GraphVertexComponent>();
                    var vertexPosition = cVertex.Position;
                    
                    if (PointInSector(position, rotation, agentRadius, agentAperture, vertexPosition))
                    {
                        // Проверим, что между агентом и вершиной нет препятствий
                    }
                }
            }
        }

        private bool PointInSector(float3 position, quaternion rotation, float radius, float aperture, float3 point)
        {
            // Проверим, что точка находится в пределах радиуса
            if (math.distance(position, point) > radius)
                return false;
            
            // Проверим, что точка находится в пределах угла aperture, при повороте rotation
            var direction = math.normalize(point - position);
            var forward = math.forward(rotation);
            var right = math.cross(forward, math.up());
            var left = math.cross(forward, right);
            var angle = math.degrees(math.acos(math.dot(forward, direction)));
            if (angle > aperture / 2f)
                return false;
            
            return true;
        }
    }
}