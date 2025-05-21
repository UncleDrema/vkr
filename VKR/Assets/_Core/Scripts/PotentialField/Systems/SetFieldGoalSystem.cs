using System;
using Game.PotentialField.Components;
using Game.PotentialField.Events;
using Game.PotentialField.Requests;
using Game.PotentialField.Tags;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Game.PotentialField.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SetFieldGoalSystem : UpdateSystem
    {
        private Filter _agents;
        
        public override void OnAwake()
        {
            _agents = World.Filter
                .With<PotentialFieldComponent>()
                .With<SetFieldGoalSelfRequest>()
                .With<MapPositionComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var agent in _agents)
            {
                ref var cPotentialField = ref agent.GetComponent<PotentialFieldComponent>();
                ref var cSetGoalRequest = ref agent.GetComponent<SetFieldGoalSelfRequest>();
                ref var cMapPosition = ref agent.GetComponent<MapPositionComponent>();

                var gX = cSetGoalRequest.X;
                var gY = cSetGoalRequest.Y;
                
                var width = cPotentialField.Width;
                var height = cPotentialField.Height;
                
                // Получаем цель
                var goalIndex = gX + gY * width;
                if (goalIndex >= 0 && goalIndex < cPotentialField.Potentials.Length)
                {
                    bool isValid;
                    int tryCount = 0;
                    for (int i = 0; i < width * height; i++)
                    {
                        if (!cPotentialField.Fixed[i])
                            cPotentialField.Potentials[i] = 0;
                    }
                    
                    do
                    {
                        tryCount++;
                        cPotentialField.GoalX = gX;
                        cPotentialField.GoalY = gY;
                    
                        var fixedCopy = new bool[width * height];
                        Array.Copy(cPotentialField.Fixed, fixedCopy, width * height);
                        fixedCopy[goalIndex] = true;
                    
                        var potentialsCopy = new double[width * height];
                        Array.Copy(cPotentialField.Potentials, potentialsCopy, width * height);
                        potentialsCopy[goalIndex] = 1;

                        var fixedSpan = fixedCopy.AsSpan();
                        var potentialsSpan = potentialsCopy.AsSpan();
                    
                        Utils.GaussSeidel(
                            ref potentialsSpan,
                            ref fixedSpan,
                            width,
                            height,
                            1e-4,
                            math.normalize(new float2(
                                UnityEngine.Random.Range(-1f, 1f),
                                UnityEngine.Random.Range(-1f, 1f))
                            ),
                            cPotentialField.Epsilon,
                            10000
                        );
                        
                        isValid = potentialsCopy[cMapPosition.Y * width + cMapPosition.X] > 0;

                        if (isValid)
                        {
                            if (!agent.Has<MovingToGoalTag>())
                            {
                                agent.AddComponent<MovingToGoalTag>();
                            }
                            Array.Copy(potentialsCopy, cPotentialField.Potentials, width * height);
                        }
                    } while (!isValid && tryCount < cSetGoalRequest.RetryCount);

                    if (isValid)
                    {
                        agent.AddComponent<SetGoalSuccessEvent>();
                    }
                    else
                    {
                        Debug.LogError($"SetFieldGoalSystem: Failed to set goal after {cSetGoalRequest.RetryCount} attempts");
                        agent.AddComponent<SetGoalFailEvent>();
                        cPotentialField.GoalX = -1;
                        cPotentialField.GoalY = -1;
                    }
                }
                else
                {
                    Debug.LogError($"SetFieldGoalSystem: Invalid goal index {goalIndex} for field with size {width}x{height}");
                }
            }
        }
    }
}