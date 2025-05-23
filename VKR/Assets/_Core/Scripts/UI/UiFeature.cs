using Game.UI.Systems;
using Scellecs.Morpeh.Addons.Feature;

namespace Game.UI
{
    public class UiFeature : LateUpdateFeature
    {
        protected override void Initialize()
        {
            AddSystem(new InitUiSystem());
            AddSystem(new UpdateUiSystem());
        }
    }
}