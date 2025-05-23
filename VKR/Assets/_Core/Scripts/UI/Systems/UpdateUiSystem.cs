using Game.PotentialField.Events;
using Game.UI.Components;
using Game.UI.Tags;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;

namespace Game.UI.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UpdateUiSystem : LateUpdateSystem
    {
        private Filter _ui;
        private Filter _agentsCreatedEvents;
        
        public override void OnAwake()
        {
            _ui = World.Filter
                .With<UiComponent>()
                .With<InitializedUiTag>()
                .Build();
            _agentsCreatedEvents = World.Filter
                .With<AgentsCreatedEvent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var ui in _ui)
            {
                ref var cUi = ref ui.GetComponent<UiComponent>();
                
                if (cUi.Ui == null)
                    continue;
                
                cUi.Ui.UpdateUi(deltaTime);
                
                foreach (var req in _agentsCreatedEvents)
                {
                    ref var cCreatedEvent = ref req.GetComponent<AgentsCreatedEvent>();
                    cUi.Ui.SetAgents(cCreatedEvent.Agents);
                }
            }
        }
    }
}