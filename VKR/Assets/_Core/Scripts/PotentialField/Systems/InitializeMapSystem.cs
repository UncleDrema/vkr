using Game.PotentialField.Components;
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

namespace Game.PotentialField.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class InitializeMapSystem : UpdateSystem
    {
        private readonly SimulationService _simulationService;
        
        private Filter _initRequests;
        private Filter _obstacles;

        public InitializeMapSystem(SimulationService simulationService)
        {
            _simulationService = simulationService;
        }

        public override void OnAwake()
        {
            _initRequests = World.Filter
                .With<TransformComponent>()
                .With<GlobalMapComponent>()
                .With<InitializeMapSelfRequest>()
                .Build();

            _obstacles = World.Filter
                .With<TransformComponent>()
                .With<ObstacleComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_simulationService.CurrentSimulationMode != SimulationMode.PotentialFieldMovement)
                return;
            
            foreach (var initReq in _initRequests)
            {
                ref var cTransform = ref initReq.GetComponent<TransformComponent>();
                ref var cMap = ref initReq.GetComponent<GlobalMapComponent>();
                
                var w = cMap.Width;
                var h = cMap.Height;
                var cellSize = cMap.CellSize;
                var cellSizeVector = new float3(cellSize, 0, cellSize);
                var mapPosition = cTransform.Position() - new float3(w * cellSize, 0, h * cellSize) / 2;
                
                // 1) Создадим массив для хранения разметки карты
                cMap.Map = new NativeArray<MapElementType>(w * h, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                
                // 2) Инициализируем карту препятствиями и границей
                for (int i = 0; i < w * h; i++)
                {
                    cMap.Map[i] = MapElementType.Empty;
                }
                
                // 3) Отметить препятствия
                foreach (var obstacle in _obstacles)
                {
                    ref var cObstacle = ref obstacle.GetComponent<ObstacleComponent>();
                    ref var cObstacleTransform = ref obstacle.GetComponent<TransformComponent>();
                    var obstacleBounds = new Bounds(cObstacleTransform.Position() + (float3)cObstacle.Bounds.center, cObstacle.Bounds.size);
                    
                    for (int x = 0; x < w; x++)
                    {
                        for (int y = 0; y < h; y++)
                        {
                            var cellBounds = new Bounds(mapPosition + new float3((x + 0.5f) * cellSize, 0, (y + 0.5f) * cellSize), cellSizeVector);
                            if (cellBounds.Intersects(obstacleBounds))
                            {
                                cMap.Map[x + y * w] = MapElementType.Obstacle;
                            }
                        }
                    }
                }
                
                // 4) Отметить границу карты
                for (int x = 0; x < w; x++)
                {
                    cMap.Map[x] = MapElementType.Border;
                    cMap.Map[(h - 1) * w + x] = MapElementType.Border;
                }

                for (int y = 0; y < h; y++)
                {
                    cMap.Map[y * w] = MapElementType.Border;
                    cMap.Map[y * w + (w - 1)] = MapElementType.Border;
                }
            }
        }
    }
}