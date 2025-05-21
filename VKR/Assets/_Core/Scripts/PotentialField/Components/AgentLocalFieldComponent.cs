using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Game.PotentialField.Components
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct AgentLocalFieldComponent : IComponent
    {
        public int Radius;
        public int Size;
        public double Epsilon;

        public double[] Potentials;
        public bool[] Fixed;
    }
}
