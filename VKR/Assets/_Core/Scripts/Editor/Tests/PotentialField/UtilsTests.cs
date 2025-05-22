using System;
using Game.PotentialField;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Editor.Tests.PotentialField
{
    /// <summary>
    /// Тест работы вспомогательных алгоритмов
    /// </summary>
    public class UtilsTests
    {
        private void ClearMatrix(int width, int height, ref Span<double> matrix, ref Span<bool> fixedValues)
        {
            for (int i = 0; i < width * height; i++)
            {
                matrix[i] = 0;
                fixedValues[i] = false;
            }
        }

        private void InitMatrix(int width, int height, ref Span<double> matrix, ref Span<bool> fixedValues, Func<int, int, (double Value, bool IsFixed)> producer)
        {
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var (value, isFixed) = producer(x, y);
                var index = Utils.GetIndex(x, y, width, height);
                matrix[index] = value;
                fixedValues[index] = isFixed;
            }
        }
        
        private void SetFixedZeroBorder(int width, int height, ref Span<double> matrix, ref Span<bool> fixedValues)
        {
            for (int x = 0; x < width; x++)
            {
                SetFixedValue(x, 0, 0, ref matrix, ref fixedValues, width, height);
                SetFixedValue(x, height - 1, 0, ref matrix, ref fixedValues, width, height);
            }
            for (int y = 0; y < height; y++)
            {
                SetFixedValue(0, y, 0, ref matrix, ref fixedValues, width, height);
                SetFixedValue(width - 1, y, 0, ref matrix, ref fixedValues, width, height);
            }
        }

        private void SetFixedValue(int x, int y, double value, ref Span<double> matrix, ref Span<bool> fixedValues, int width, int height)
        {
            var index = Utils.GetIndex(x, y, width, height);
            fixedValues[index] = true;
            matrix[index] = value;
        }
        
        private double GetValue(int x, int y, ref Span<double> matrix, int width, int height)
        {
            var index = Utils.GetIndex(x, y, width, height);
            return matrix[index];
        }

        private void AssertForEach(int width, int height, ref Span<double> matrix, ref Span<bool> fixedValues,
            Predicate<(int x, int y, double value, bool isFixed)> predicate)
        {
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var index = Utils.GetIndex(x, y, width, height);
                var value = matrix[index];
                var isFixed = fixedValues[index];
                Assert.IsTrue(predicate((x, y, value, isFixed)), $"Failed at ({x}, {y})");
            }
        }
        
        [TestCase(5, 5, 0.25f, 100)]
        [TestCase(100, 100, 0.1f, 100)]
        [TestCase(250, 250, 0.01f, 100)]
        public void TestGaussSeidel(int width, int height, float fixedRate, int repeats)
        {
            Span<double> matrix = new double[width * height];
            Span<bool> fixedValues = new bool[width * height];

            for (int i = 0; i < repeats; i++)
            {
                // Arrange
                // Заполним матрицу с заданной плотностью фиксированных значений
                InitMatrix(width, height, ref matrix, ref fixedValues, (x, y) =>
                {
                    var isFixed = UnityEngine.Random.value < fixedRate;
                    return (isFixed ? 1 : 0, isFixed);
                });

                // Act
                Utils.GaussSeidel(ref matrix, ref fixedValues, width, height, 0.01, new double2(0, 0), 1, 100);

                // Assert
                // Фиксированные значения не должны изменяться, а нефиксированные должны быть в (0, 1)
                AssertForEach(width, height, ref matrix, ref fixedValues, tuple =>
                {
                    var (x, y, value, isFixed) = tuple;
                    if (isFixed)
                    {
                        return value == 1;
                    }
                    else
                    {
                        return value is >= 0 and <= 1;
                    }
                });
            }
        }

        [TestCase(5, 5, 2, 2)]
        [TestCase(5, 5, 2, 3)]
        [TestCase(5, 5, 3, 2)]
        [TestCase(6, 6, 2, 4)]
        [TestCase(6, 6, 4, 2)]
        public void TestGradientTowardsFixedValue(int width, int height, int fixedValueX, int fixedValueY)
        {
            // Arrange
            Span<double> matrix = new double[width * height];
            Span<bool> fixedValues = new bool[width * height];
            ClearMatrix(width, height, ref matrix, ref fixedValues);
            SetFixedZeroBorder(width, height, ref matrix, ref fixedValues);
            SetFixedValue(fixedValueX, fixedValueY, 1, ref matrix, ref fixedValues, width, height);
            
            // Act
            Utils.GaussSeidel(ref matrix, ref fixedValues, width, height, 0.01, new double2(0, 0), 1, 100);
            
            // print matrix
            Debug.Log("Matrix after GaussSeidel:");
            for (int y = 0; y < height; y++)
            {
                string row = "";
                for (int x = 0; x < width; x++)
                {
                    var index = Utils.GetIndex(x, y, width, height);
                    row += $"{matrix[index]:F2} ";
                }
                Debug.Log(row);
            }
            
            // Assert
            // Проверим, что градиент направлен к фиксированному значению
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                // Получаем градиент
                var gradient = Utils.GetGradient(ref matrix, x, y, width, height);
                
                // Проверяем, что градиент направлен к фиксированному значению
                if (!fixedValues[Utils.GetIndex(x, y, width, height)])
                {
                    if (x < fixedValueX)
                    {
                        Assert.IsTrue(gradient.x > 0, $"Gradient at ({x}, {y}) should be positive in x direction.");
                    }
                    else if (x > fixedValueX)
                    {
                        Assert.IsTrue(gradient.x < 0, $"Gradient at ({x}, {y}) should be negative in x direction.");
                    }
                    
                    if (y < fixedValueY)
                    {
                        Assert.IsTrue(gradient.y > 0, $"Gradient at ({x}, {y}) should be positive in y direction.");
                    }
                    else if (y > fixedValueY)
                    {
                        Assert.IsTrue(gradient.y < 0, $"Gradient at ({x}, {y}) should be negative in y direction.");
                    }
                }
            }
        }
        
        [Test]
        public void Test_GetGradientDoesNotFailInEveryPoint()
        {
            // Arrange
            int width = 5;
            int height = 5;
            Span<double> matrix = new double[width * height];
            Span<bool> fixedValues = new bool[width * height];
            ClearMatrix(width, height, ref matrix, ref fixedValues);
            
            // Act
            Utils.GaussSeidel(ref matrix, ref fixedValues, width, height, 0.01, new double2(0, 0), 1, 100);
            
            // Assert
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                // Получаем градиент
                var gradient = Utils.GetGradient(ref matrix, x, y, width, height);
                
                // Проверяем что градиент не равен NaN
                Assert.IsFalse(double.IsNaN(gradient.x) || double.IsNaN(gradient.y), $"Gradient at ({x}, {y}) is NaN.");
            }
        }
    }
}