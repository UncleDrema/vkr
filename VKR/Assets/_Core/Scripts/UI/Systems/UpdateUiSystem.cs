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
        
        public override void OnAwake()
        {
            _ui = World.Filter
                .With<UiComponent>()
                .With<InitializedUiTag>()
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
            }
        }
    }
}