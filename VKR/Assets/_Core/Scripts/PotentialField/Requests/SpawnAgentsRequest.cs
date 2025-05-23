using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Game.PotentialField.Requests
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct SpawnAgentsRequest : IComponent
    {
        public List<AgentDescription> Agents;
    }

    [Serializable]
    public class AgentDescription
    {
        public float3 Position { get; set; }
        public float Speed;
    }
}
