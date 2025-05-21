using Game.Common.Components;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Transform.Components;
using Unity.IL2CPP.CompilerServices;

namespace Game.Common.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TransformApplySystem : LateUpdateSystem
    {
        private Filter _transformFilter;

        public override void OnAwake()
        {
            _transformFilter = World.Filter
                .With<GameObjectComponent>()
                .With<TransformComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var entity in _transformFilter)
            {
                ref var cTransform = ref entity.GetComponent<TransformComponent>();
                ref var cGo = ref entity.GetComponent<GameObjectComponent>();

                cGo.Transform.position = cTransform.Position();
                cGo.Transform.rotation = cTransform.Rotation();
            }
        }
    }
}