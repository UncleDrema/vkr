using Game.Common.Components;
using Game.PotentialField.Components;
using Game.PotentialField.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Providers;
using Scellecs.Morpeh.Transform.Components;
using UnityEditor;
using UnityEngine;

namespace Game.PotentialField.Entities
{
    public class MapEntity : CodeUniversalProvider
    {
        protected override void RegisterTypes()
        {
            RegisterType<GameObjectComponent>();
            RegisterType<TransformComponent>();
            RegisterType<GlobalMapComponent>();
            RegisterType<InitializeMapSelfRequest>();
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            if (Entity == default)
                return;
            var cMap = Entity.GetComponent<GlobalMapComponent>();
            var width = cMap.Width;
            var height = cMap.Height;
            var cellSize = cMap.CellSize;
            var map = cMap.Map;
            
            var position = transform.position;
            position -= new Vector3(width * cellSize, 0, height * cellSize) / 2;
            var size = new Vector3(width * cellSize, 0, height * cellSize);
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(position + size / 2, size);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = FromMapElementType(map.IsCreated ? map[x + y * width] : MapElementType.Empty);
                    var cellPosition = position + new Vector3(x * cellSize, 0, y * cellSize) + new Vector3(cellSize / 2, 0, cellSize / 2);
                    Gizmos.DrawWireCube(cellPosition, new Vector3(cellSize, 0, cellSize));
                }
            }
        }

        private Color FromMapElementType(MapElementType type)
        {
            return type switch
            {
                MapElementType.Empty => Color.yellow,
                MapElementType.Border => Color.red,
                MapElementType.Obstacle => Color.blue,
                _ => Color.black
            };
        }
#endif
    }
}