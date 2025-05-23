using Game.MapGraph.Components;
using Game.Planning.Components;
using Game.PotentialField.Events;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Game.Planning.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ClearAgentPatrolTargetOnReachedOrFailedSystem : UpdateSystem
    {
        private Filter _reachedGoalAgents;
        private Filter _failedSetGoalAgents;
        
        public override void OnAwake()
        {
            _reachedGoalAgents = World.Filter
                .With<GoalReachedEvent>()
                .With<AgentPatrolComponent>()
                .Build();
            _failedSetGoalAgents = World.Filter
                .With<SetGoalFailEvent>()
                .With<AgentPatrolComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var agent in _reachedGoalAgents)
            {
                ref var cPatrol = ref agent.GetComponent<AgentPatrolComponent>();

                cPatrol.GoalVertex = default;
            }
            
            foreach (var agent in _failedSetGoalAgents)
            {
                ref var cPatrol = ref agent.GetComponent<AgentPatrolComponent>();
                var goalVertex = cPatrol.GoalVertex;
                ref var cGraphVertex = ref goalVertex.GetComponent<GraphVertexComponent>();
                cGraphVertex.LastSelectFailedTime = Time.time;
                
                cPatrol.GoalVertex = default;
            }
        }
    }
}