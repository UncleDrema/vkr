using Game.MapGraph.Components;
using Game.Movement.Components;
using Game.Planning.Components;
using Game.PotentialField.Events;
using Game.SimulationControl;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Transform.Components;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Game.PotentialField.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SimpleMovementSystem : UpdateSystem
    {
        private readonly SimulationService _simulationService;
        
        private Filter _agents;

        public SimpleMovementSystem(SimulationService simulationService)
        {
            _simulationService = simulationService;
        }

        public override void OnAwake()
        {
            _agents = World.Filter
                .With<AgentPatrolComponent>()
                .With<TransformComponent>()
                .With<MovementComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_simulationService.CurrentSimulationMode != SimulationMode.SimpleMovement)
                return;
            
            foreach (var agent in _agents)
            {
                ref var cTransform = ref agent.GetComponent<TransformComponent>();
                ref var cMovement = ref agent.GetComponent<MovementComponent>();
                ref var cPatrol = ref agent.GetComponent<AgentPatrolComponent>();
                
                if (cPatrol.GoalVertex == default)
                    continue;
                
                var position = cTransform.Position();
                var goal = cPatrol.GoalVertex;

                ref var cVertex = ref goal.GetComponent<GraphVertexComponent>();
                var goalPosition = cVertex.Position;
                
                var direction = math.normalize(goalPosition - position);
                var distance = math.distance(position, goalPosition);

                cMovement.Direction = direction;

                if (distance < 0.1f)
                {
                    agent.AddComponent<GoalReachedEvent>();
                }
            }
        }
    }
}