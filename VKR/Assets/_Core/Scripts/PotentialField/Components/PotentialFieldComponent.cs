using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Game.PotentialField.Components
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct PotentialFieldComponent : IComponent
    {
        public float3 Center;
        public int Width, Height;
        public float CellSize;      // длина стороны ячейки
        public double[] Potentials;  // 1D-рядок, Length = Width*Height
        public bool[] Fixed;   // true для статических препятствий / границ
        public double Epsilon;       // ϵ — интенсивность возмущения
        public int GoalX, GoalY; // координаты цели в глобальной карте
    }
}
