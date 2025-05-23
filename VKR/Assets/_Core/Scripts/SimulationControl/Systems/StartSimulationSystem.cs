using System;
using System.Collections.Generic;
using Game.MapGraph.Components;
using Game.MapGraph.Requests;
using Game.Planning.Components;
using Game.PotentialField.Components;
using Game.PotentialField.Events;
using Game.PotentialField.Requests;
using Game.SceneManagement.Api;
using Game.SimulationControl.Components;
using Game.SimulationControl.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Feature.Events;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Game.SimulationControl.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class StartSimulationSystem : UpdateSystem
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly SimulationService _simulationService;

        private Filter _simulations;
        private Filter _startSimulationRequests;
        private Filter _initAfterSceneLoadedRequests;
        private Filter _collectStatisticsRequests;
        private Filter _agentsCreatedEvents;
        
        public StartSimulationSystem(ISceneLoader sceneLoader, SimulationService simulationService)
        {
            _sceneLoader = sceneLoader;
            _simulationService = simulationService;
        }
        
        public override void OnAwake()
        {
            _simulations = World.Filter
                .With<SimulationComponent>()
                .Build();
            _startSimulationRequests = World.Filter
                .With<StartSimulationRequest>()
                .Build();
            _initAfterSceneLoadedRequests = World.Filter
                .With<SimulationComponent>()
                .With<InitializeAfterSceneLoadedRequest>()
                .Build();
            _collectStatisticsRequests = World.Filter
                .With<CollectStatisticsRequest>()
                .Build();
            _agentsCreatedEvents = World.Filter
                .With<AgentsCreatedEvent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var req in _collectStatisticsRequests)
            {
                SaveSimulationEvents();
            }
            
            foreach (var startRequest in _startSimulationRequests)
            {
                ref var cStartRequest = ref startRequest.GetComponent<StartSimulationRequest>();
                var mode = cStartRequest.Mode;
                SaveSimulationEvents();
                ClearSimulation();
                _simulationService.CurrentSimulationMode = mode;
                _simulationService.CurrentAgents = cStartRequest.Agents;
            }

            foreach (var initReq in _initAfterSceneLoadedRequests)
            {
                ref var cSimulation = ref initReq.GetComponent<SimulationComponent>();
                
                cSimulation.TimePassed = 0;
                cSimulation.SimulationEvents = new Dictionary<float, Dictionary<string, object>>();
                var simulationMode = _simulationService.CurrentSimulationMode;
                switch (simulationMode)
                {
                    case SimulationMode.PotentialFieldMovement:
                        StartPotentialFieldMovementSimulation();
                        break;
                    case SimulationMode.SimpleMovement:
                        StartSimpleMovementSimulation();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            foreach (var agentsCreatedEv in _agentsCreatedEvents)
            {
                CreateZonesForAgents();
            }
        }

        private void SaveSimulationEvents()
        {
            foreach (var simulation in _simulations)
            {
                ref var cSimulation = ref simulation.GetComponent<SimulationComponent>();
                
                // serialize to json file simulation_events.json
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(cSimulation.SimulationEvents, Newtonsoft.Json.Formatting.Indented);
                var time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string path;
                if (Application.isEditor)
                {
                    path = Application.persistentDataPath +
                           $"/simulation_{_simulationService.CurrentSimulationMode}_{cSimulation.TimePassed:F2}s_{time}.json";
                }
                else
                {
                    // start from current directory
                    path = Application.dataPath +
                           $"/simulation_{_simulationService.CurrentSimulationMode}_{cSimulation.TimePassed:F2}s_{time}.json";
                }

                System.IO.File.WriteAllText(path, json);
            }
        }

        private void StartSimpleMovementSimulation()
        {
            Debug.Log("Start Simple Movement Simulation");
            ref var cCreateAgents = ref World.CreateEventEntity<SpawnAgentsRequest>();
            cCreateAgents.Agents = _simulationService.CurrentAgents;
        }

        private void StartPotentialFieldMovementSimulation()
        {
            Debug.Log("Start Potential Field Movement Simulation");
            ref var cCreateAgents = ref World.CreateEventEntity<SpawnAgentsRequest>();
            cCreateAgents.Agents = _simulationService.CurrentAgents;
        }
        
        private void CreateZonesForAgents()
        {
            var agentsFilter = World.Filter.With<AgentLocalFieldComponent>().Build();
            var agentsCount = agentsFilter.GetLengthSlow();
            Debug.Log($"Agents count: {agentsCount}");
            ref var cCreateZonesRequest = ref World.CreateEventEntity<InitializeGraphZonesRequest>();
            cCreateZonesRequest.ZoneCount = agentsCount;
            foreach (var agent in agentsFilter)
            {
                ref var cPatrol = ref agent.AddComponent<AgentPatrolComponent>();
                cPatrol.GoalVertex = default;
            }
        }

        private void ClearSimulation()
        {
            World.GetStash<GraphVertexComponent>().RemoveAll();
            World.GetStash<GraphComponent>().RemoveAll();
            World.GetStash<ThreatEventComponent>().RemoveAll();
            World.GetStash<ZoneComponent>().RemoveAll();
            World.GetStash<GlobalMapComponent>().RemoveAll();
            World.GetStash<PotentialFieldComponent>().RemoveAll();
            World.GetStash<AgentPatrolComponent>().RemoveAll();
            World.GetStash<MapPositionComponent>().RemoveAll();
            World.GetStash<AgentLocalFieldComponent>().RemoveAll();
            World.GetStash<DynamicObstacleComponent>().RemoveAll();
            _sceneLoader.ReloadActiveScene();
        }
    }
}