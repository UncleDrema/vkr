using Game.PotentialField.Components;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Transform.Components;
using Unity.Mathematics;

namespace Game.PotentialField
{
    public class MapService
    {
        private Filter _maps;
        
        public void Initialize(World world)
        {
            _maps = world.Filter.With<GlobalMapComponent>().Build();
        }
        
        public int2 WorldToMapPosition(float3 pos)
        {
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

                return new int2(x, y);
            }
            
            return int2.zero;
        }
    }
}