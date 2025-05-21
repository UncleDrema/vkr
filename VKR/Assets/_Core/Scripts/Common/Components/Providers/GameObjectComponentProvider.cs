using Scellecs.Morpeh.Providers;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Game.Common.Components.Providers
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class GameObjectComponentProvider 
        : MonoProvider<GameObjectComponent>, ISerializationCallbackReceiver
    {
        public void OnBeforeSerialize()
        {
            ref var data = ref GetData();
            data.Object = gameObject;
            data.Transform = gameObject.transform;
        }

        public void OnAfterDeserialize() { }
    }
}
