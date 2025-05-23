using Game.MapGraph;
using Game.MapGraph.Components;
using Game.Movement.Components;
using Game.Planning.Components;
using Game.PotentialField.Components;
using Game.PotentialField.Events;
using Game.PotentialField.Tags;
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
    public sealed class LocalFieldMovementSystem : UpdateSystem
    {
        private readonly SimulationService _simulationService;
        private readonly GraphService _graphService;
        
        private Filter _agents;
        private Filter _stoppedMovementAgents;

        public LocalFieldMovementSystem(SimulationService simulationService, GraphService graphService)
        {
            _simulationService = simulationService;
            _graphService = graphService;
        }

        public override void OnAwake()
        {
            _agents = World.Filter
                .With<AgentLocalFieldComponent>()
                .With<AgentPatrolComponent>()
                .With<MovementComponent>()
                .With<TransformComponent>()
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
            if (_simulationService.CurrentSimulationMode != SimulationMode.PotentialFieldMovement)
                return;
            
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
                    ref var cPatrol = ref agent.GetComponent<AgentPatrolComponent>();
                    ref var cTransform = ref agent.GetComponent<TransformComponent>();
                    var goalVertex = cPatrol.GoalVertex;
                    var nearestVertex = _graphService.GetNearestVertex(cTransform.Position(), out _);
                    
                    var path = _graphService.GetPath(nearestVertex, goalVertex);
                    if (path.Count == 0)
                    {
                        cMovement.Direction = float3.zero;
                    }
                    else if (path.Count == 1)
                    {
                        var targetVertex = path[0];
                        ref var cTargetVertex = ref targetVertex.GetComponent<GraphVertexComponent>();
                        var targetPosition = cTargetVertex.Position;
                        
                        var direction = targetPosition - cTransform.Position();
                        cMovement.Direction = math.normalize(direction);
                    }
                    else
                    {
                        var targetVertex = path[1];
                        ref var cTargetVertex = ref targetVertex.GetComponent<GraphVertexComponent>();
                        var targetPosition = cTargetVertex.Position;
                        
                        var direction = targetPosition - cTransform.Position();
                        cMovement.Direction = math.normalize(direction);
                    }
                }
                else
                {
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