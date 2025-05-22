using Game.Movement.Components;
using Game.PotentialField.Components;
using Game.PotentialField.Events;
using Game.PotentialField.Tags;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Game.PotentialField.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class LocalFieldMovementSystem : UpdateSystem
    {
        private Filter _agents;
        private Filter _stoppedMovementAgents;
        
        public override void OnAwake()
        {
            _agents = World.Filter
                .With<AgentLocalFieldComponent>()
                .With<MovementComponent>()
                .With<MovingToGoalTag>()
                .Build();
            
            _stoppedMovementAgents = World.Filter
                .With<AgentLocalFieldComponent>()
                .With<MovementComponent>()
                .With<GoalReachedEvent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var agent in _agents)
            {
                ref var cLocalField = ref agent.GetComponent<AgentLocalFieldComponent>();
                ref var cMovement = ref agent.GetComponent<MovementComponent>();
                
                var r = cLocalField.Radius;
                var s = cLocalField.Size;
                
                var centerX = r;
                var centerY = r;
                var centerIndex = centerY * s + centerX;

                double pl = cLocalField.Potentials[centerIndex - 1];
                double pr = cLocalField.Potentials[centerIndex + 1];
                double pb = cLocalField.Potentials[centerIndex - s];
                double pt = cLocalField.Potentials[centerIndex + s];
                
                var gradientX = (float)(pr - pl) / 2f;
                var gradientY = (float)(pt - pb) / 2f;

                var gradient = new float3(gradientX, 0, gradientY);
                
                if (math.lengthsq(gradient) < 1e-40f)
                {
                    cMovement.Direction = float3.zero;
                }
                else
                {
                    // Normalize the gradient vector to get the direction
                    cMovement.Direction = math.normalize(gradient);
                }
            }
            
            foreach (var agent in _stoppedMovementAgents)
            {
                ref var cMovement = ref agent.GetComponent<MovementComponent>();
                
                // Stop the agent
                cMovement.Direction = float3.zero;
            }
        }
    }
}