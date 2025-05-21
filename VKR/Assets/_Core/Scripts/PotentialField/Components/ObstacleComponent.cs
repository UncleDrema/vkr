using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Game.PotentialField.Components
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct ObstacleComponent : IComponent, IDrawGizmos
    {
        public Bounds Bounds; // границы препятствия
        
        public void OnDrawGizmos(GameObject gameObject)
        {
            var transform = gameObject.transform;
            var position = transform.position;
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(position + Bounds.center, Bounds.size);
            
            // draw small cube at center of cell
            var cellPos = new Vector3(Bounds.center.x, 0, Bounds.center.z);
            var worldPos = position + cellPos;
            Gizmos.DrawCube(worldPos, new Vector3(0.1f, 0.1f, 0.1f));
        }
    }
}
