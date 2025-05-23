using Game.Movement.Components;
using Game.PotentialField.Events;
using Game.PotentialField.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Feature.Events;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Collections;
using Scellecs.Morpeh.Providers;
using Scellecs.Morpeh.Transform.Components;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Game.PotentialField.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SpawnAgentsSystem : UpdateSystem
    {
        private readonly AgentConfig _agentConfig;
        
        private Filter _spawnAgentsRequests;

        public SpawnAgentsSystem(AgentConfig agentConfig)
        {
            _agentConfig = agentConfig;
        }

        public override void OnAwake()
        {
            _spawnAgentsRequests = World.Filter.With<SpawnAgentsRequest>().Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var req in _spawnAgentsRequests)
            {
                ref var cReq = ref req.GetComponent<SpawnAgentsRequest>();

                foreach (var agentDesc in cReq.Agents)
                {
                    var agent = Object.Instantiate(_agentConfig.AgentPrefab);

                    if (!EntityProvider.map.TryGetValue(agent.GetInstanceID(), out var item))
                    {
                        Debug.LogError($"Not found entity with id {agent.GetInstanceID()}");
                        return;
                    }

                    var entity = item.entity;
                    ref var cTransform = ref entity.GetComponent<TransformComponent>();
                    cTransform.SetPosition(agentDesc.Position);
                    ref var cMovement = ref entity.GetComponent<MovementComponent>();
                    cMovement.Speed = agentDesc.Speed;
                }

                ref var cCreatedEvent = ref World.CreateEventEntity<AgentsCreatedEvent>();
                cCreatedEvent.Agents = cReq.Agents;
            }
        }
    }
}