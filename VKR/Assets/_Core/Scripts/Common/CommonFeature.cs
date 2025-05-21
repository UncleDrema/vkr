using Game.Common.Components;
using Game.Common.Systems;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Feature;

namespace Game.Common
{
    public class CommonFeature : LateUpdateFeature
    {
        protected override void Initialize()
        {
            TransformCache.RefreshCache(World.Default);

            AddSystem(new TransformApplySystem());

            MarkComponentsDisposable();
        }

        private void MarkComponentsDisposable()
        {
            World.Default.GetStash<GameObjectComponent>().AsDisposable();
        }
    }
}