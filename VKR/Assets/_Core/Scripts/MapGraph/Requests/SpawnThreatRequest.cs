﻿using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Game.MapGraph.Requests
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct SpawnThreatRequest : IComponent
    {
        public Entity TargetVertex;
        public float ThreatLevel;
        public float ThreadDuration;
        public ThreatDecayType DecayType;
    }
}
