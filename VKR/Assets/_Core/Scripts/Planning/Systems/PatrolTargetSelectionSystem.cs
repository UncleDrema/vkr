using Game.MapGraph;
using Game.MapGraph.Components;
using Game.Planning.Components;
using Game.PotentialField.Requests;
using Game.SimulationControl;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Transform.Components;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using SimulationMode = Game.SimulationControl.SimulationMode;

namespace Game.Planning.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PatrolTargetSelectionSystem : UpdateSystem
    {
        private readonly GraphService _graphService;
        private readonly PatrolService _patrolService;
        private readonly SimulationService _simulationService;
        
        private Filter _agents;
        
        public PatrolTargetSelectionSystem(GraphService graphService, PatrolService patrolService, SimulationService simulationService)
        {
            _graphService = graphService;
            _patrolService = patrolService;
            _simulationService = simulationService;
        }
        
        public override void OnAwake()
        {
            _agents = World.Filter
                .With<AgentPatrolComponent>()
                .With<TransformComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!_patrolService.InPatrolMode)
                return;
            
            if (_simulationService.CurrentSimulationMode != SimulationMode.PotentialFieldMovement)
                return;
            
            foreach (var agent in _agents)
            {
                ref var cPatrol = ref agent.GetComponent<AgentPatrolComponent>();
                
                if (cPatrol.GoalVertex != default || cPatrol.Zones == null)
                    continue;
                
                ref var cTransform = ref agent.GetComponent<TransformComponent>();

                var position = cTransform.Position();
                var zones = cPatrol.Zones;
                
                NativeList<Entity> vertices = new NativeList<Entity>(Allocator.Temp);
                foreach (var zone in zones)
                {
                    ref var cZone = ref zone.GetComponent<ZoneComponent>();
                    foreach (var vertex in cZone.Vertices)
                    {
                        if (vertices.Contains(vertex))
                            continue;
                        
                        vertices.Add(vertex);
                    }
                }

                Entity currentVertex = _graphService.GetNearestVertex(position, out _);

                // метрика: score = λU·U(v) - λD·d(a,v)
                const float lambdaU = 20f;
                const float lambdaD = 0.5f;
                const float lambdaFailPenalty = 200f;
                const float failTimeThreshold = 10f;
                float bestScore = float.NegativeInfinity;
                Entity bestV = default;
                float3 bestPosition = float3.zero;
                foreach (var vertex in vertices)
                {
                    ref var cVertex = ref vertex.GetComponent<GraphVertexComponent>();
                    float Uv = cVertex.Threat;
                    float d  = _graphService.GetDistance(currentVertex, vertex);
                    float timeFromFailedSelection = Time.time - cVertex.LastSelectFailedTime;
                    float failPenalty = (1 - math.tanh(timeFromFailedSelection - failTimeThreshold));
                    
                    // Брать слишком близко или слишком далеко - плохо, воспользуемся формулой, дающей пик около 3 и падение к 1
                    /*
                     * y=x^{1-c}-1+\operatorname{abs}\left(30b\right)+1+g
                     * b=\operatorname{abs}\left(\tanh\left(x-3\right)\right)-1
                     * c=\frac{\left(\tanh\left(x-6\right)+1\right)}{2}
                     * g=\frac{\left(\left(-\tanh\left(x-10\right)\right)-1\right)x}{2}
                     */
                    float b = math.abs(math.tanh(d - 4)) - 1;
                    float c = (math.tanh(d - 8) + 1) / 2;
                    float g = (math.tanh(d - 10) - 1) * d / 2;
                    float y = math.pow(d, 1 - c) - 1 + math.abs(30 * b) + 1 + g;
                    float score = lambdaU * Uv + lambdaD * y - lambdaFailPenalty * failPenalty;
                    
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestV     = vertex;
                        bestPosition = cVertex.Position;
                    }
                }
                
                cPatrol.GoalVertex = bestV;
                ref var cSetGoalRequest = ref agent.AddComponent<SetGoalByPositionSelfRequest>();
                cSetGoalRequest.Position = bestPosition;
                //Debug.Log($"Assigned goal {bestV} to agent {agent} with score {bestScore} and position {bestPosition}");
            }
        }
    }
}