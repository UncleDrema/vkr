using System;
using System.Collections.Generic;
using Game.PotentialField.Requests;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Game.PotentialField.Events
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct AgentsCreatedEvent : IComponent
    {
        public List<AgentDescription> Agents;
    }
}
