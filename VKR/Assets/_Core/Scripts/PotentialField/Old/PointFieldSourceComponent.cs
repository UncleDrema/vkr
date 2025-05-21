using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Game.PotentialField.Old
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct PointFieldSourceComponent : IComponent, IDrawGizmos
    {
        public float Radius;
        public float Strength;
        public float DecaySpeed;
        
        private const int CircleSegments = 64;
        private const float StrengthEpsilon = 0.1f;
        private static readonly Color AttractColor = Color.blue;
        private static readonly Color RepelColor = Color.red;
        
        public void OnDrawGizmos(GameObject go)
        {
            if (DecaySpeed < Mathf.Epsilon)
                return;
            
            var center = go.transform.position;
            float maxStrength = Mathf.Abs(Strength);
            float radius = Radius;
            
            var color = Strength > 0 ? RepelColor : AttractColor;

            // 1. Внутренняя зона насыщения
            DrawCircle(center, radius, color);

            // 2. Визуализация экспоненциального затухания
            float distance = radius;
            while (true)
            {
                distance += 0.1f;
                float relativeStrength = Mathf.Exp(-(distance - radius) / DecaySpeed);
                if (relativeStrength * maxStrength < StrengthEpsilon)
                    break;

                Color faded = color * relativeStrength;
                faded.a = 1.0f;
                DrawCircle(center, distance, faded);
            }
        }
        
        private void DrawCircle(Vector3 center, float radius, Color color)
        {
            Gizmos.color = color;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);
            for (int i = 1; i <= CircleSegments; i++)
            {
                float angle = (2 * Mathf.PI / CircleSegments) * i;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }
}
