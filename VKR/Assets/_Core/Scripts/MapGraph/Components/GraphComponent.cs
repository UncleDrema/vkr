using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;

namespace Game.MapGraph.Components
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct GraphComponent : IComponent
    {
        public List<Entity> Vertices;
    }
}
