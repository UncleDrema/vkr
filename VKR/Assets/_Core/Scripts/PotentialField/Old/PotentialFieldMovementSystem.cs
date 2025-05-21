using Game.Movement.Components;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Game.PotentialField.Old
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PotentialFieldMovementSystem : UpdateSystem
    {
        private Filter _movingObjects;
        
        public override void OnAwake()
        {
            _movingObjects = World.Filter
                .With<MovementComponent>()
                .With<PotentialFieldValueComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var target in _movingObjects)
            {
                ref var cMovement = ref target.GetComponent<MovementComponent>();
                ref var cFieldValue = ref target.GetComponent<PotentialFieldValueComponent>();

                var fieldValue = cFieldValue.Value;
                fieldValue.y = 0;
                if (math.lengthsq(fieldValue) < 0.01f)
                {
                    cMovement.Direction = float3.zero;
                }
                else
                {
                    cMovement.Direction = math.normalize(fieldValue);
                }
            }
        }
    }
}