using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Game.PotentialField.Requests
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct SetFieldGoalSelfRequest : IComponent
    {
        public int X;
        public int Y;
        public int RetryCount;
    }
}
