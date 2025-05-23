using Game.UI.Components;
using Scellecs.Morpeh;

namespace Game.UI
{
    public class UiEntity : HierarchyCodeUniversalProvider
    {
        protected override void RegisterTypes()
        {
            RegisterType<UiComponent>();
        }
    }
}