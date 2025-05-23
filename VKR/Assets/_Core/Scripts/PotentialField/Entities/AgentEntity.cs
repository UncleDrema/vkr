using Game.Common.Components;
using Game.Movement.Components;
using Game.PotentialField.Components;
using Game.PotentialField.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Transform.Components;
using TriInspector;
using Unity.Mathematics;
using UnityEngine;

namespace Game.PotentialField.Entities
{
    public class AgentEntity : HierarchyCodeUniversalProvider
    {
        public bool GlobalField = true;
        public bool DrawPotential = true;
        public bool DrawGradient = true;
        [Range(0.0001f, 1f)]
        public float PotentialScale = 0.4f;
        
        protected override void RegisterTypes()
        {
            RegisterType<GameObjectComponent>();
            RegisterType<TransformComponent>();
            RegisterType<MovementComponent>();
            RegisterType<AgentLocalFieldComponent>();
            RegisterType<PotentialFieldComponent>();
            RegisterType<InitializeAgentSelfRequest>();
            RegisterType<MapPositionComponent>();
            RegisterType<DynamicObstacleComponent>();
            RegisterType<MovementAnimatorComponent>();
        }

        [Button]
        public void SetGoal(int x, int y)
        {
            ref var cSetGoalRequest = ref Entity.AddComponent<SetFieldGoalSelfRequest>();
            cSetGoalRequest.X = x;
            cSetGoalRequest.Y = y;
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            if (Entity == default)
                return;
            
            ref var cField = ref Entity.GetComponent<PotentialFieldComponent>();
            ref var cLocalField = ref Entity.GetComponent<AgentLocalFieldComponent>();
            ref var cAgentPosition = ref Entity.GetComponent<MapPositionComponent>();

            if (GlobalField)
            {
                var globalFieldCenter = (Vector3)cField.Center;
                var leftDownCorner = globalFieldCenter -
                                     new Vector3(cField.Width * cField.CellSize, 0,
                                         cField.Height * cField.CellSize) / 2;
                var size = new Vector3(cField.Width * cField.CellSize, 0.1f, cField.Height * cField.CellSize);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(globalFieldCenter, size);

                for (int x = 0; x < cField.Width; x++)
                {
                    for (int y = 0; y < cField.Height; y++)
                    {
                        var index = y * cField.Width + x;
                        var potential = cField.Potentials[index];
                        potential = math.pow(potential, PotentialScale);
                        var cellPos = new Vector3(x * cField.CellSize + cField.CellSize / 2, 0,
                            y * cField.CellSize + cField.CellSize / 2);
                        var worldPos = leftDownCorner + cellPos;

                        if (DrawPotential)
                        {
                            Gizmos.color = Color.Lerp(Color.green, Color.red, (float)potential);
                            Gizmos.DrawCube(worldPos,
                                new Vector3(cField.CellSize / 2, (float)potential * 3f, cField.CellSize / 2));

                            if (cField.Fixed[index])
                            {
                                Gizmos.color = Color.blue;
                                Gizmos.DrawCube(worldPos,
                                    new Vector3(cField.CellSize / 1.5f, 3f, cField.CellSize / 1.5f));
                            }
                        }

                        if (DrawGradient)
                        {
                            // Рассчитаем градиент
                            double left = x > 0 ? cField.Potentials[index - 1] : cField.Potentials[index];
                            double right = x < cField.Width - 1
                                ? cField.Potentials[index + 1]
                                : cField.Potentials[index];
                            double down = y > 0 ? cField.Potentials[index - cField.Width] : cField.Potentials[index];
                            double up = y < cField.Height - 1
                                ? cField.Potentials[index + cField.Width]
                                : cField.Potentials[index];
                            double dx = (right - left) / (2 * cField.CellSize);
                            double dy = (up - down) / (2 * cField.CellSize);
                            var gradient = new Vector3((float)dx, 0, (float)dy);
                            gradient = math.normalize(gradient);
                            Gizmos.color = Color.black;
                            Gizmos.DrawLine(worldPos + Vector3.up * 1, worldPos + Vector3.up * 1 + gradient * 0.5f);
                            // draw arrow cap with offset from arrow tip
                            var midPoint = worldPos + Vector3.up * 1 + gradient * 0.5f;
                            var arrowLength = 0.05f;
                            var arrowWidth = 0.1f;
                            var arrowTip = midPoint + gradient * arrowLength;
                            var arrowLeft = Quaternion.Euler(0, 45, 0) * gradient * arrowWidth;
                            var arrowRight = Quaternion.Euler(0, -45, 0) * gradient * arrowWidth;
                            Gizmos.DrawLine(midPoint, arrowTip);
                            Gizmos.DrawLine(arrowTip, arrowTip + arrowLeft);
                            Gizmos.DrawLine(arrowTip, arrowTip + arrowRight);
                            Gizmos.DrawLine(midPoint + arrowLeft, midPoint + arrowRight);
                        }
                    }
                }
            }
            else
            {
                var globalFieldCenter = (Vector3)cField.Center;
                var leftDownCorner = globalFieldCenter -
                                     new Vector3(cField.Width * cField.CellSize, 0,
                                         cField.Height * cField.CellSize) / 2;
                var agentX = cAgentPosition.X;
                var agentY = cAgentPosition.Y;
                var agentPosFromCorner = new Vector3(agentX * cField.CellSize, 0, agentY * cField.CellSize);
                var localFieldCenter = leftDownCorner + agentPosFromCorner;
                var localFieldLeftDownCorner = localFieldCenter -
                                     new Vector3(cLocalField.Size * cField.CellSize, 0,
                                         cLocalField.Size * cField.CellSize) / 2;
                var localFieldSize = new Vector3(cLocalField.Size * cField.CellSize, 0.1f, cLocalField.Size * cField.CellSize);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(localFieldCenter, localFieldSize);

                for (int x = 0; x < cLocalField.Size; x++)
                {
                    for (int y = 0; y < cLocalField.Size; y++)
                    {
                        var index = y * cLocalField.Size + x;
                        var potential = cLocalField.Potentials[index];
                        potential = math.pow(potential, PotentialScale);
                        var cellPos = new Vector3(x * cField.CellSize + cField.CellSize / 2, 0,
                            y * cField.CellSize + cField.CellSize / 2);
                        var worldPos = localFieldLeftDownCorner + cellPos;
                        
                        if (DrawPotential)
                        {
                            Gizmos.color = Color.Lerp(Color.green, Color.red, (float)potential);
                            Gizmos.DrawCube(worldPos,
                                new Vector3(cField.CellSize / 2, (float)potential * 3f, cField.CellSize / 2));

                            if (cLocalField.Fixed[index])
                            {
                                if (cLocalField.Potentials[index] > 0.5)
                                {
                                    Gizmos.color = Color.magenta;
                                }
                                else
                                {
                                    Gizmos.color = Color.blue;
                                }
                                Gizmos.DrawCube(worldPos,
                                    new Vector3(cField.CellSize / 1.5f, 3f, cField.CellSize / 1.5f));
                            }
                        }
                        
                        if (DrawGradient)
                        {
                            // Рассчитаем градиент
                            double left = x > 0 ? cLocalField.Potentials[index - 1] : cLocalField.Potentials[index];
                            double right = x < cLocalField.Size - 1
                                ? cLocalField.Potentials[index + 1]
                                : cLocalField.Potentials[index];
                            double down = y > 0 ? cLocalField.Potentials[index - cLocalField.Size] : cLocalField.Potentials[index];
                            double up = y < cLocalField.Size - 1
                                ? cLocalField.Potentials[index + cLocalField.Size]
                                : cLocalField.Potentials[index];
                            double dx = (right - left) / (2 * cField.CellSize);
                            double dy = (up - down) / (2 * cField.CellSize);
                            var gradient = new Vector3((float)dx, 0, (float)dy);
                            gradient = math.normalize(gradient);
                            Gizmos.color = Color.black;
                            Gizmos.DrawLine(worldPos + Vector3.up * 1, worldPos + Vector3.up * 1 + gradient * 0.5f);
                            // draw arrow cap with offset from arrow tip
                            var midPoint = worldPos + Vector3.up * 1 + gradient * 0.5f;
                            var arrowLength = 0.05f;
                            var arrowWidth = 0.1f;
                            var arrowTip = midPoint + gradient * arrowLength;
                            var arrowLeft = Quaternion.Euler(0, 45, 0) * gradient * arrowWidth;
                            var arrowRight = Quaternion.Euler(0, -45, 0) * gradient * arrowWidth;
                            Gizmos.DrawLine(midPoint, arrowTip);
                            Gizmos.DrawLine(arrowTip, arrowTip + arrowLeft);
                            Gizmos.DrawLine(arrowTip, arrowTip + arrowRight);
                            Gizmos.DrawLine(midPoint + arrowLeft, midPoint + arrowRight);
                        }
                    }
                }
                return;

                for (int x = 0; x < cField.Width; x++)
                {
                    for (int y = 0; y < cField.Height; y++)
                    {
                        var index = y * cField.Width + x;
                        var potential = cField.Potentials[index];
                        potential = math.pow(potential, PotentialScale);
                        var cellPos = new Vector3(x * cField.CellSize + cField.CellSize / 2, 0,
                            y * cField.CellSize + cField.CellSize / 2);
                        var worldPos = leftDownCorner + cellPos;

                        if (DrawPotential)
                        {
                            Gizmos.color = Color.Lerp(Color.green, Color.red, (float)potential);
                            Gizmos.DrawCube(worldPos,
                                new Vector3(cField.CellSize / 2, (float)potential * 3f, cField.CellSize / 2));

                            if (cField.Fixed[index])
                            {
                                Gizmos.color = Color.blue;
                                Gizmos.DrawCube(worldPos,
                                    new Vector3(cField.CellSize / 1.5f, 3f, cField.CellSize / 1.5f));
                            }
                        }

                        if (DrawGradient)
                        {
                            // Рассчитаем градиент
                            double left = x > 0 ? cField.Potentials[index - 1] : cField.Potentials[index];
                            double right = x < cField.Width - 1
                                ? cField.Potentials[index + 1]
                                : cField.Potentials[index];
                            double down = y > 0 ? cField.Potentials[index - cField.Width] : cField.Potentials[index];
                            double up = y < cField.Height - 1
                                ? cField.Potentials[index + cField.Width]
                                : cField.Potentials[index];
                            double dx = (right - left) / (2 * cField.CellSize);
                            double dy = (up - down) / (2 * cField.CellSize);
                            var gradient = new Vector3((float)dx, 0, (float)dy);
                            gradient = math.normalize(gradient);
                            Gizmos.color = Color.black;
                            Gizmos.DrawLine(worldPos + Vector3.up * 1, worldPos + Vector3.up * 1 + gradient * 0.5f);
                            // draw arrow cap with offset from arrow tip
                            var midPoint = worldPos + Vector3.up * 1 + gradient * 0.5f;
                            var arrowLength = 0.2f;
                            var arrowWidth = 0.1f;
                            var arrowTip = midPoint + gradient * arrowLength;
                            var arrowLeft = Quaternion.Euler(0, 45, 0) * gradient * arrowWidth;
                            var arrowRight = Quaternion.Euler(0, -45, 0) * gradient * arrowWidth;
                            Gizmos.DrawLine(midPoint, arrowTip);
                            Gizmos.DrawLine(arrowTip, arrowTip + arrowLeft);
                            Gizmos.DrawLine(arrowTip, arrowTip + arrowRight);
                            Gizmos.DrawLine(midPoint + arrowLeft, midPoint + arrowRight);
                        }
                    }
                }
            }
        }
#endif
    }
}