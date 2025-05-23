using System.Collections.Generic;
using Game.Planning.Components;
using Game.Planning.Requests;
using Game.PotentialField.Events;
using Game.SimulationControl;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Transform.Components;
using Unity.IL2CPP.CompilerServices;

namespace Game.Planning.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SimpleMovementPlanningSystem : UpdateSystem
    {
        private readonly SimulationService _simulationService;
        
        private Filter _goalReachedEvents;
        private Filter _agentsWithoutPathComponents;

        public SimpleMovementPlanningSystem(SimulationService simulationService)
        {
            _simulationService = simulationService;
        }

        public override void OnAwake()
        {
            _goalReachedEvents = World.Filter
                .With<TransformComponent>()
                .With<AgentPatrolComponent>()
                .With<GoalReachedEvent>()
                .Build();
            _agentsWithoutPathComponents = World.Filter
                .With<AgentPatrolComponent>()
                .Without<SimpleMovementPathComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_simulationService.CurrentSimulationMode != SimulationMode.SimpleMovement)
                return;
            
            foreach (var agent in _goalReachedEvents)
            {
                agent.AddComponent<SelectSimpleMovementGoalRequest>();
            }

            foreach (var agent in _agentsWithoutPathComponents)
            {
                ref var cPath = ref agent.AddComponent<SimpleMovementPathComponent>();
                cPath.Path = new List<Entity>();

                agent.AddComponent<SelectSimpleMovementGoalRequest>();
            }
        }
    }
}