using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Game.MapGraph.Components
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct GraphComponent : IComponent
    {
        public List<Entity> Vertices;
        public List<int2> Edges;
        public Dictionary<Entity, Dictionary<Entity, float>> VertexDistances;
    }
}
