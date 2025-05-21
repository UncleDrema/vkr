using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Game.Common.Components
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct GameObjectComponent : IComponent, IValidatableWithGameObject, IDisposable
    {
        public GameObject Object;
        public Transform Transform;
        public bool KeepGameObjectOnComponentRemove; 

        public void OnValidate(GameObject gameObject)
        {
            Object = gameObject;
            Transform = gameObject.transform;
        }
        
        public void Dispose()
        {
            if (Object != null && !KeepGameObjectOnComponentRemove)
                UnityEngine.Object.Destroy(Object);            
        }
    }
}
