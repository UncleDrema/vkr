using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Common;
using Game.MapGraph;
using Game.MapGraph.Requests;
using Game.Movement;
using Game.Movement.Components;
using Game.Planning;
using Game.PotentialField;
using Game.PotentialField.Components;
using Game.PotentialField.Requests;
using Game.SceneManagement.Api;
using Game.SimulationControl;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons;
using Scellecs.Morpeh.Addons.Feature;
using Scellecs.Morpeh.Addons.Feature.Events;
using Scellecs.Morpeh.Transform.Components;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Editor.Tests
{
    public class MethodTestUtils
    {
        private class StubSceneLoader : ISceneLoader
        {
            public bool IsGameLoaded { get; }
            public async UniTask LoadGameScene() {
            }

            public async UniTask LoadScene(string scenePath, bool isActive = true) { }

            public async UniTask UnloadScene(string scenePath) { }

            public async UniTask UnloadActiveScene() { }

            public async UniTask UnloadActiveSceneNextFrame() { }

            public async UniTask ReloadActiveScene() { }

            public async UniTask UnloadActiveThenLoadScene(string scenePath, bool isActive = true) { }
        }
        
        public static World CreateWorld()
        {
            MorpehAddons.Initialize();
            World world = EcsTestUtils.CreateWorld();

            var mapService = new MapService();
            var graphService = new GraphService();
            var patrolService = new PatrolService();
            var simulationService = new SimulationService();
            var sceneLoaderService = new StubSceneLoader();
            var agentConfig = TestUtils.LoadAsset<AgentConfig>();

            int order = 0;
            world.AddFeature(order++, new MapGraphFeature(graphService, simulationService));
            world.AddFeature(order++, new PotentialFieldFeature(mapService, simulationService, graphService, agentConfig));
            world.AddFeature(order++, new MovementFeature());
            world.AddFeature(order++, new PlanningFeature(graphService, patrolService, simulationService));
            world.AddFeature(order++, new SimulationControlFeature(sceneLoaderService, simulationService));

            world.AddFeature(order++, new HierarchyFeature());
            world.AddFeature(order++, new TransformFeature());
            world.AddFeature(order++, new CommonFeature());
            
            return world;
        }

        public static void CreateThreat(World world, float3 position, float threat, float duration, ThreatDecayType decayType)
        {
            ref var cThreatSpawnRequest = ref world.CreateEventEntity<SpawnThreatByPositionRequest>();
            cThreatSpawnRequest.Position = position;
            cThreatSpawnRequest.ThreatLevel = threat;
            cThreatSpawnRequest.ThreatDuration = duration;
            cThreatSpawnRequest.DecayType = decayType;
        }

        public static Entity CreateGraph(World world, List<Vector3> vertices, List<Edge> edges, float segmentLength)
        {
            var initGraphRequest = world.CreateEntity();
            
            ref var cInitRequest = ref initGraphRequest.AddComponent<InitializeGraphRequest>();
            cInitRequest.DesiredSegmentLength = segmentLength;
            cInitRequest.Nodes = vertices;
            cInitRequest.Edges = edges;

            return initGraphRequest;
        }

        public static Entity CreateZones(World world, int zoneCount)
        {
            var initZonesRequest = world.CreateEntity();
            
            ref var cInitRequest = ref initZonesRequest.AddComponent<InitializeGraphZonesRequest>();
            
            cInitRequest.ZoneCount = zoneCount;
            
            return initZonesRequest;
        }

        public static Entity CreateObstacle(World world, float3 center, float2 size)
        {
            var obstacle = world.CreateEntity();
            
            ref var cTransform = ref obstacle.AddComponent<TransformComponent>();
            ref var cObstacle = ref obstacle.AddComponent<ObstacleComponent>();
            
            cTransform.SetPosition(center);
            cTransform.SetRotation(quaternion.identity);
            cTransform.LocalScale = new float3(1, 1, 1);
            cTransform.LocalToWorld = float4x4.TRS(cTransform.LocalPosition, cTransform.LocalRotation, cTransform.LocalScale);
            
            cObstacle.Bounds = new Bounds(center, new float3(size.x, 1, size.y));
            
            return obstacle;
        }

        public static Entity CreateMap(World world, float3 center, int width, int height, float cellSize)
        {
            var map = world.CreateEntity();
            
            ref var cTransform = ref map.AddComponent<TransformComponent>();
            ref var cMap = ref map.AddComponent<GlobalMapComponent>();
            ref var cInitRequest = ref map.AddComponent<InitializeMapSelfRequest>();
            
            cTransform.SetPosition(center);
            cTransform.SetRotation(quaternion.identity);
            cTransform.LocalScale = new float3(1, 1, 1);
            cTransform.LocalToWorld = float4x4.TRS(cTransform.LocalPosition, cTransform.LocalRotation, cTransform.LocalScale);
            
            cMap.Width = width;
            cMap.Height = height;
            cMap.CellSize = cellSize;

            return map;
        }
        
        public static Entity SetAgentGoal(Entity agent, float3 position)
        {
            ref var cSetGoalReq = ref agent.AddComponent<SetGoalByPositionSelfRequest>();
            cSetGoalReq.Position = position;
            return agent;
        }

        public static Entity CreateAgent(World world, float3 position, float speed)
        {
            var agent = world.CreateEntity();
            
            ref var cTransform = ref agent.AddComponent<TransformComponent>();
            ref var cPotentialField = ref agent.AddComponent<PotentialFieldComponent>();
            ref var cAgentLocalField = ref agent.AddComponent<AgentLocalFieldComponent>();
            ref var cInitRequest = ref agent.AddComponent<InitializeAgentSelfRequest>();
            ref var cDynamicObstacle = ref agent.AddComponent<DynamicObstacleComponent>();
            ref var cMovement = ref agent.AddComponent<MovementComponent>();
            ref var cMapPosition = ref agent.AddComponent<MapPositionComponent>();
            
            cTransform.SetPosition(position);
            cTransform.SetRotation(quaternion.identity);
            cTransform.LocalScale = new float3(1, 1, 1);
            cTransform.LocalToWorld = float4x4.TRS(cTransform.LocalPosition, cTransform.LocalRotation, cTransform.LocalScale);
            
            cMovement.Speed = speed;
            cMovement.MaxRadiansDelta = 1f;
            
            cDynamicObstacle.SizeX = 1;
            cDynamicObstacle.SizeY = 1;

            return agent;
        }
    }
}