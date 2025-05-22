using System;
using Game.Common;
using Game.MapGraph;
using Game.Movement;
using Game.Planning;
using Game.PotentialField;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Feature;
using Scellecs.Morpeh.Addons.Feature.Unity;
using Scellecs.Morpeh.Addons.Unity.VContainer;
using UnityEngine;
using VContainer;

namespace Game.GameStartup
{
    [DefaultExecutionOrder(-1)]
    internal class EcsInstaller : BaseFeaturesInstaller
    {
        private IObjectResolver _container;

        [Inject]
        private void Inject(IObjectResolver container)
        {
            _container = container;
        }
        
        protected override void InitializeShared()
        {
            
        }

        protected override UpdateFeature[] InitializeUpdateFeatures()
        {
            return new UpdateFeature[]
            {
                _container.CreateFeature<MapGraphFeature>(),
                _container.CreateFeature<PotentialFieldFeature>(),
                _container.CreateFeature<MovementFeature>(),
                _container.CreateFeature<PlanningFeature>(),
            };
        }

        protected override FixedUpdateFeature[] InitializeFixedUpdateFeatures()
        {
            return Array.Empty<FixedUpdateFeature>();
        }

        protected override LateUpdateFeature[] InitializeLateUpdateFeatures()
        {
            return new LateUpdateFeature[]
            {
                _container.CreateFeature<HierarchyFeature>(),
                _container.CreateFeature<TransformFeature>(),
                _container.CreateFeature<CommonFeature>(),
            };
        }
    }
}