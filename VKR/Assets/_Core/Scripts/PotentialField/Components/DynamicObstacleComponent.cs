using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Game.PotentialField.Components
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct DynamicObstacleComponent : IComponent
    {
        public int SizeX;
        public int SizeY;
    }
}
