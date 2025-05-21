using Game.PotentialField.Components;
using Game.PotentialField.Requests;
using Game.PotentialField.Tags;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;

namespace Game.PotentialField.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ClearFieldGoalSystem : UpdateSystem
    {
        private Filter _fields;
        
        public override void OnAwake()
        {
            _fields = World.Filter
                .With<PotentialFieldComponent>()
                .With<ClearFieldGoalRequest>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var field in _fields)
            {
                ref var cField = ref field.GetComponent<PotentialFieldComponent>();
                
                cField.GoalX = -1;
                cField.GoalY = -1;

                for (int i = 0; i < cField.Width * cField.Height; i++)
                {
                    if (!cField.Fixed[i])
                    {
                        cField.Potentials[i] = 0;
                    }
                }

                if (field.Has<MovingToGoalTag>())
                    field.RemoveComponent<MovingToGoalTag>();
            }
        }
    }
}