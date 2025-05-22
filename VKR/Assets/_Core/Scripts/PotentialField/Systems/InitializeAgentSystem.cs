using Game.Planning.Components;
using Game.PotentialField.Components;
using Game.PotentialField.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Transform.Components;
using Unity.IL2CPP.CompilerServices;

namespace Game.PotentialField.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class InitializeAgentSystem : UpdateSystem
    {
        private Filter _maps;
        private Filter _agents;
        
        public override void OnAwake()
        {
            _agents = World.Filter
                .With<AgentLocalFieldComponent>()
                .With<TransformComponent>()
                .With<PotentialFieldComponent>()
                .With<InitializeAgentSelfRequest>()
                .Build();
            _maps = World.Filter
                .With<GlobalMapComponent>()
                .With<TransformComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var agent in _agents)
            {
                ref var cPatrol = ref agent.AddComponent<AgentPatrolComponent>();
                ref var cLocalField = ref agent.GetComponent<AgentLocalFieldComponent>();
                ref var cTransform = ref agent.GetComponent<TransformComponent>();
                ref var cPotentialField = ref agent.GetComponent<PotentialFieldComponent>();
                ref var cInitRequest = ref agent.GetComponent<InitializeAgentSelfRequest>();

                cPatrol.GoalVertex = default;
                cLocalField.Radius = 10;
                cLocalField.Size = cLocalField.Radius * 2 + 1;
                cLocalField.Epsilon = 0.05;
                cLocalField.Potentials = new double[cLocalField.Size * cLocalField.Size];
                for (int i = 0; i < cLocalField.Size * cLocalField.Size; i++)
                {
                    cLocalField.Potentials[i] = 0;
                }
                cLocalField.Fixed = new bool[cLocalField.Size * cLocalField.Size];
                for (int i = 0; i < cLocalField.Size * cLocalField.Size; i++)
                {
                    cLocalField.Fixed[i] = false;
                }

                foreach (var map in _maps)
                {
                    ref var cMap = ref map.GetComponent<GlobalMapComponent>();
                    ref var cMapTransform = ref map.GetComponent<TransformComponent>();
                    
                    var width = cMap.Width;
                    var height = cMap.Height;
                    var cellSize = cMap.CellSize;
                    
                    cPotentialField.Width = width;
                    cPotentialField.Height = height;
                    cPotentialField.CellSize = cellSize;
                    cPotentialField.Center = cMapTransform.Position();
                    cPotentialField.Potentials = new double[width * height];
                    cPotentialField.Fixed = new bool[width * height];
                    cPotentialField.Epsilon = 0.1;
                    
                    // Установим потенциал в 0 для всех ячеек и зафиксируем на препятствиях и границе
                    for (int i = 0; i < width * height; i++)
                    {
                        var mapElement = cMap.Map[i];
                        cPotentialField.Potentials[i] = 0;
                        cPotentialField.Fixed[i] = mapElement is MapElementType.Border or MapElementType.Obstacle;
                    }
                }
            }
        }
    }
}