using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Game.MapGraph.Components
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct ThreatEventComponent : IComponent
    {
        public Entity TargetVertex;
        public float ThreatLevel;
        public float ExistTime;
        public float Duration;
        public ThreatDecayType DecayType;
    }
}
