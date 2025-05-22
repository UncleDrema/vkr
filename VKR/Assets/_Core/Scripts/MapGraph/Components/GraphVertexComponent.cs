using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Game.MapGraph.Components
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct GraphVertexComponent : IComponent
    {
        public float3 Position;
        public List<Entity> Neighbors;
        public float Threat;
        public float LastObservationTime;
        public Entity Zone;
    }
}
