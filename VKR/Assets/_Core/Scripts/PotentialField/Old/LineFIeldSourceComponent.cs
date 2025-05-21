using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Game.PotentialField.Old
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct LineFieldSourceComponent : IComponent, IDrawGizmos
    {
        public float Radius;
        public float Strength;
        public float3 LocalStart;
        public float3 LocalEnd;
        public float DecaySpeed;
        
        private const float StrengthEpsilon = 0.1f;
        private static readonly Color AttractColor = Color.blue;
        private static readonly Color RepelColor = Color.red;
        
        public void OnDrawGizmos(GameObject go)
        {
            if (DecaySpeed < Mathf.Epsilon)
                return;
            var tf = go.transform;
            var p0 = tf.TransformPoint(LocalStart);
            var p1 = tf.TransformPoint(LocalEnd);
            var radius = Radius;

            var lineDir = (p1 - p0).normalized;
            var normal = Vector3.Cross(lineDir, Vector3.up).normalized;
            
            var maxStrength = Mathf.Abs(Strength);
            
            var color = Strength > 0 ? RepelColor : AttractColor;

            // Центральная зона постоянной силы (толстая линия)
            Gizmos.color = color;
            Gizmos.DrawLine(p0 + normal * radius, p1 + normal * radius);
            Gizmos.DrawLine(p0 - normal * radius, p1 - normal * radius);

            // Градиент затухания от линии наружу
            float distance = radius;

            while (true)
            {
                distance += 0.1f;
                float relativeStrength = Mathf.Exp(-(distance - radius) / DecaySpeed);
                if (relativeStrength * maxStrength < StrengthEpsilon)
                    break;

                Color faded = color * relativeStrength;
                faded.a = 1.0f;
                Gizmos.color = faded;

                Gizmos.DrawLine(p0 + normal * distance, p1 + normal * distance);
                Gizmos.DrawLine(p0 - normal * distance, p1 - normal * distance);
            }
        }
    }
}
