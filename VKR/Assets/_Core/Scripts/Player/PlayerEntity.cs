using Game.Common.Components;
using Game.Movement.Components;
using Scellecs.Morpeh;

namespace Game.Player
{
    public class PlayerEntity : HierarchyCodeUniversalProvider
    {
        protected override void RegisterTypes()
        {
            RegisterType<GameObjectComponent>();
            RegisterType<MovementComponent>();
        }

        private void Start()
        {
            ref var cMovement = ref Entity.GetComponent<MovementComponent>();
            cMovement.Speed = 1;
        }
    }
}