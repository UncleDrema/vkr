using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Game.SimulationControl.Components
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct SimulationComponent : IComponent
    {
        public float TimePassed;
        public Dictionary<float, Dictionary<string, object>> SimulationEvents;

        public void AddEvent(string name, object value)
        {
            if (!SimulationEvents.TryGetValue(TimePassed, out var events))
            {
                events = new Dictionary<string, object>();
                SimulationEvents[TimePassed] = events;
            }
            
            events[name] = value;
        }
    }
}
