using Game.Movement.Components;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Movement.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UpdateAnimationSystem : UpdateSystem
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private Filter _animators;
        
        public override void OnAwake()
        {
            _animators = World.Filter.With<MovementAnimatorComponent>().With<MovementComponent>().Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var animator in _animators)
            {
                ref var cAnimator = ref animator.GetComponent<MovementAnimatorComponent>();
                ref var cMovement = ref animator.GetComponent<MovementComponent>();
                
                cAnimator.Animator.SetFloat(Speed, math.length(cMovement.Direction));
            }
        }
    }
}