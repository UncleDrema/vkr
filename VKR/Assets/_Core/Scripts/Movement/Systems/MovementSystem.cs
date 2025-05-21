using Game.Movement.Components;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Native;
using Scellecs.Morpeh.Transform.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using Unity.Mathematics;

namespace Game.Movement.Systems
{
    [BurstCompile]
    internal struct MovementSystemJob : IJobParallelFor
    {
        public static readonly float SqrEpsilon = math.EPSILON * math.EPSILON;
        
        [ReadOnly]
        public NativeFilter Entities;

        public NativeStash<TransformComponent> TransformComponents;
            
        [ReadOnly]
        public NativeStash<MovementComponent> MovementComponents;

        public float DeltaTime;
                
        public void Execute(int index)
        {
            var entity = Entities[index];
            ref var transform = ref TransformComponents.Get(entity);
            ref var movement = ref MovementComponents.Get(entity);

            var translation = GetTranslation(movement);
            transform.Translate(translation);

            if (movement.MaxRadiansDelta > 0f && !movement.Direction.Equals(float3.zero))
                transform.LocalRotation = GetRotation(transform.LocalRotation, movement);
        }
            
        private float3 GetTranslation(MovementComponent movement)
        {
            float3 delta = movement.Direction * movement.Speed * DeltaTime;
            return delta;
        }
        
        private quaternion GetRotation(quaternion currentRotation, MovementComponent movement)
        {
            movement.Direction *= (1f - (float3)movement.FreezeRotation);
            quaternion newRotation = currentRotation;
                
            float3 currentLookDirection = math.mul(currentRotation, math.forward());
            float sqrMagnitude = math.distancesq(currentLookDirection, movement.Direction);
                
            if (sqrMagnitude > SqrEpsilon)
            {
                newRotation = math.slerp(
                    currentRotation, 
                    quaternion.LookRotation(movement.Direction, math.up()),
                    movement.MaxRadiansDelta * DeltaTime);
            }

            return newRotation;
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class MovementSystem : UpdateSystem
    {
        private Filter _movementEntities;
        
        private Stash<TransformComponent> _transformComponents;
        private Stash<MovementComponent> _movementComponents;
        
        public override void OnAwake()
        {
            _movementEntities = World.Filter
                .With<MovementComponent>()
                .With<TransformComponent>()
                .Build();

            _transformComponents = World.GetStash<TransformComponent>();
            _movementComponents = World.GetStash<MovementComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            var nativeFilter = _movementEntities.AsNative();
            var job = new MovementSystemJob()
            {
                Entities = nativeFilter,
                TransformComponents = _transformComponents.AsNative(),
                MovementComponents = _movementComponents.AsNative(),
                DeltaTime = deltaTime,
            };

            job.Schedule(nativeFilter.length, 64).Complete();
        }
    }
}