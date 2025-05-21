using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Game.PotentialField.Components
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct MapPositionComponent : IComponent
    {
        public int X;
        public int Y;
    }
}
