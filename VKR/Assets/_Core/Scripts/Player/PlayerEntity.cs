using Game.Common.Components;
using Game.Movement.Components;
using Game.PotentialField.Components;
using Game.PotentialField.Old;
using Scellecs.Morpeh;

namespace Game.Player
{
    public class PlayerEntity : HierarchyCodeUniversalProvider
    {
        protected override void RegisterTypes()
        {
            RegisterType<GameObjectComponent>();
            RegisterType<MovementComponent>();
            RegisterType<PotentialFieldValueComponent>();
        }

        private void Start()
        {
            ref var cMovement = ref Entity.GetComponent<MovementComponent>();
            cMovement.Speed = 1;
        }
    }
}