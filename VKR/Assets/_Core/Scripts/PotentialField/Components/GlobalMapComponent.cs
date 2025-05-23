using System;
using Scellecs.Morpeh;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Game.PotentialField.Components
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct GlobalMapComponent : IComponent
#if UNITY_EDITOR
        , IDrawGizmosSelected
    #endif
    {
        public int Width, Height;
        public float CellSize;
        public NativeArray<MapElementType> Map;

#if UNITY_EDITOR
        public void OnDrawGizmosSelected(GameObject gameObject)
        {
            if (Application.isPlaying)
                return;
            var transform = gameObject.transform;
            var position = transform.position;
            position -= new Vector3(Width * CellSize, 0, Height * CellSize) / 2;
            var size = new Vector3(Width * CellSize, 0, Height * CellSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(position + size / 2, size);
            
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var cellPosition = position + new Vector3(x * CellSize, 0, y * CellSize) + new Vector3(CellSize / 2, 0, CellSize / 2);
                    Gizmos.DrawWireCube(cellPosition, new Vector3(CellSize, 0, CellSize));
                }
            }
        }
#endif
    }
}
