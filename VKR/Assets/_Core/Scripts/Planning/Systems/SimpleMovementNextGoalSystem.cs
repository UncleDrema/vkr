using System.Collections.Generic;
using System.Linq;
using Game.MapGraph;
using Game.MapGraph.Components;
using Game.Planning.Components;
using Game.Planning.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Transform.Components;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Game.Planning.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SimpleMovementNextGoalSystem : UpdateSystem
    {
        private readonly GraphService _graphService;
        
        private Filter _selectGoalRequests;

        public SimpleMovementNextGoalSystem(GraphService graphService)
        {
            _graphService = graphService;
        }

        public override void OnAwake()
        {
            _selectGoalRequests = World.Filter
                .With<TransformComponent>()
                .With<AgentPatrolComponent>()
                .With<SimpleMovementPathComponent>()
                .With<SelectSimpleMovementGoalRequest>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var agent in _selectGoalRequests)
            {
                ref var cTransform = ref agent.GetComponent<TransformComponent>();
                ref var cPatrol = ref agent.GetComponent<AgentPatrolComponent>();
                ref var cPath = ref agent.GetComponent<SimpleMovementPathComponent>();

                if (cPath.Path.Count == 0)
                {
                    NewPath(ref cTransform, ref cPatrol, ref cPath);
                }
                var vertex = cPath.Path[0];
                cPath.Path.RemoveAt(0);
                
                Debug.Log($"Set new goal for agent {agent} to {vertex}");
                cPatrol.GoalVertex = vertex;
            }
        }

        private void NewPath(ref TransformComponent cTransform, ref AgentPatrolComponent cPatrol, ref SimpleMovementPathComponent cPath)
        {
            var zones = cPatrol.Zones;
            var vertices = new List<Entity>();
            foreach (var zone in zones)
            {
                var zoneVertices = zone.GetComponent<ZoneComponent>().Vertices;
                foreach (var vertex in zoneVertices)
                {
                    if (!vertices.Contains(vertex))
                    {
                        vertices.Add(vertex);
                    }
                }
            }
            
            // find all border vertices, that have only one connected vertice that is inside vertices list
            var borderVertices = new List<Entity>();
            foreach (var vertex in vertices)
            {
                var cVertex = vertex.GetComponent<GraphVertexComponent>();
                if (cVertex.Neighbors.Count(v => vertices.Contains(v)) == 1)
                {
                    borderVertices.Add(vertex);
                }
            }
            
            // find the oldest visited vertex
            var oldestVertex = borderVertices
                .OrderBy(v => v.GetComponent<GraphVertexComponent>().LastObservationTime)
                .First();
            
            // Построим путь от ближайшей к агенту вершины до самой старой
            var nearestVertex = _graphService.GetNearestVertex(cTransform.Position(), out _);
            
            var path = _graphService.GetPath(nearestVertex, oldestVertex);
            if (path.Count > 0)
            {
                cPath.Path = path;
            }
            else
            {
                // Если путь не найден, то просто добавим ближайшую к агенту вершину
                cPath.Path.Add(nearestVertex);
                Debug.LogWarning($"Path not found from {nearestVertex} to {oldestVertex}");
            }
        }
    }
}