using Game.PotentialField.Components;
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
    public sealed class UpdateAgentPositionSystem : UpdateSystem
    {
        private Filter _objects;
        private Filter _maps;
        
        public override void OnAwake()
        {
            _objects = World.Filter
                .With<MapPositionComponent>()
                .With<TransformComponent>()
                .Build();

            _maps = World.Filter
                .With<GlobalMapComponent>()
                .With<TransformComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var agent in _objects)
            {
                ref var cTransform = ref agent.GetComponent<TransformComponent>();
                ref var cMapPosition = ref agent.GetComponent<MapPositionComponent>();

                var pos = cTransform.Position();
                
                foreach (var map in _maps)
                {
                    ref var cMapTransform = ref map.GetComponent<TransformComponent>();
                    ref var cMap = ref map.GetComponent<GlobalMapComponent>();
                    
                   
                    var cellSize = cMap.CellSize;
                    var width = cMap.Width;
                    var height = cMap.Height;
                    var center = cMapTransform.Position();
                    
                    var leftDownCorner = center - new float3(width * cellSize, 0, height * cellSize) / 2;
                    var x = (int)((pos.x - leftDownCorner.x) / cellSize);
                    var y = (int)((pos.z - leftDownCorner.z) / cellSize);
                
                    x = math.clamp(x, 0, width - 1);
                    y = math.clamp(y, 0, height - 1);
                    cMapPosition.X = x;
                    cMapPosition.Y = y;
                }
            }
        }
    }
}