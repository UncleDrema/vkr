using Game.SimulationControl.Components;
using Game.SimulationControl.Requests;
using Scellecs.Morpeh;

namespace Game.SimulationControl.Entities
{
    public class SimulationControlEntity : HierarchyCodeUniversalProvider
    {
        protected override void RegisterTypes()
        {
            RegisterType<InitializeAfterSceneLoadedRequest>();
            RegisterType<SimulationComponent>();
        }
    }
}