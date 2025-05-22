using System;
using Unity.Mathematics;

namespace Game.PotentialField
{
    public static class Utils
    {
        public static int GetIndexRaw(int x, int y, int width, int height)
        {
            return y * width + x;
        }
        
        public static int GetIndex(int x, int y, int width, int height)
        {
            return math.clamp(GetIndexRaw(x, y, width, height), 0, width * height - 1);
        }
        
        public static float2 GetGradient(
            ref Span<double> matrix,
            int x,
            int y,
            int width,
            int height
        )
        {
            var index = GetIndexRaw(x, y, width, height);
            var pl = x > 0 ? matrix[index - 1] : matrix[index];
            var pr = x < width - 1 ? matrix[index + 1] : matrix[index];
            var pb = y > 0 ? matrix[index - width] : matrix[index];
            var pt = y < height - 1 ? matrix[index + width] : matrix[index];

            return new float2((float)(pr - pl) / 2f, (float)(pt - pb) / 2f);
        }
        
        public static void GaussSeidel(
            ref Span<double> matrix,
            ref Span<bool> fixedValues,
            int width,
            int height,
            double convergeEpsilon,
            double2 perturbationVector,
            double perturbationPower,
            int iterations
        )
        {
            Span<double> tempMatrix = stackalloc double[width * height];
            for (int i = 0; i < width * height; i++)
            {
                tempMatrix[i] = matrix[i];
            }
            bool writingToTemp = true;

            for (int it = 0; it < iterations; it++)
            {
                bool converged = true;
                
                for (int y = 1; y < height - 1; y++)
                for (int x = 1; x < width - 1; x++)
                {
                    var index = y * width + x;
                    if (fixedValues[index])
                        continue;
                     
                    double sum = 0;
                    int cnt = 0;

                    double pb = 0;
                    double pt = 0;
                    double pl = 0;
                    double pr = 0;
                    if (writingToTemp)
                    {
                        if (matrix[index - width] > 0 || !fixedValues[index - width])
                        {
                            pb = matrix[index - width];
                            cnt++;
                        }
                        if (matrix[index + width] > 0 || !fixedValues[index + width])
                        {
                            pt = matrix[index + width];
                            cnt++;
                        }
                        if (matrix[index - 1] > 0 || !fixedValues[index - 1])
                        {
                            pl = matrix[index - 1];
                            cnt++;
                        }
                        if (matrix[index + 1] > 0 || !fixedValues[index + 1])
                        {
                            pr = matrix[index + 1];
                            cnt++;
                        }
                    }
                    else
                    {
                        if (tempMatrix[index - width] > 0 || !fixedValues[index - width])
                        {
                            pb = tempMatrix[index - width];
                            cnt++;
                        }
                        if (tempMatrix[index + width] > 0 || !fixedValues[index + width])
                        {
                            pt = tempMatrix[index + width];
                            cnt++;
                        }
                        if (tempMatrix[index - 1] > 0 || !fixedValues[index - 1])
                        {
                            pl = tempMatrix[index - 1];
                            cnt++;
                        }
                        if (tempMatrix[index + 1] > 0 || !fixedValues[index + 1])
                        {
                            pr = tempMatrix[index + 1];
                            cnt++;
                        }
                    }
                    
                    if (cnt == 0)
                    {
                        continue;
                    }
                    
                    /*
                    var pb = writingToTemp ? matrix[index - width] : tempMatrix[index - width];
                    var pt = writingToTemp ? matrix[index + width] : tempMatrix[index + width];
                    var pl = writingToTemp ? matrix[index - 1] : tempMatrix[index - 1];
                    var pr = writingToTemp ? matrix[index + 1] : tempMatrix[index + 1];
                    
                    var mean = (pb + pt + pl + pr) * 0.25;
                    */
                    var mean = (pb + pt + pl + pr) / 4;
                    var perturbationGain = (perturbationPower * ((pr - pl) * perturbationVector.x + (pb - pt) * perturbationVector.y)) * 0.125;
                    
                    var newValue = mean + perturbationGain;
                    
                    if (math.abs(newValue - matrix[index]) > convergeEpsilon)
                    {
                        converged = false;
                    }
                    
                    if (writingToTemp)
                        tempMatrix[index] = newValue;
                    else
                        matrix[index] = newValue;
                }
                
                writingToTemp = !writingToTemp;
                
                if (converged)
                {
                    break;
                }
            }
            
            // Устанавливаем значения в границах, если они не фиксированы
            SetIfNotFixed(ref matrix, ref fixedValues, 0 + 0 * width, matrix[1 + 1 * width]);
            SetIfNotFixed(ref matrix, ref fixedValues, 0 + (height - 1) * width, matrix[1 + (height - 2) * width]);
            SetIfNotFixed(ref matrix, ref fixedValues, (width - 1) + 0 * width, matrix[(width - 2) + 1 * width]);
            SetIfNotFixed(ref matrix, ref fixedValues, (width - 1) + (height - 1) * width, matrix[(width - 2) + (height - 2) * width]);
            for (int x = 1; x < width - 1; x++)
            {
                SetIfNotFixed(ref matrix, ref fixedValues, x + 0 * width, matrix[x + 1 * width]);
                SetIfNotFixed(ref matrix, ref fixedValues, x + (height - 1) * width, matrix[x + (height - 2) * width]);
            }
            for (int y = 1; y < height - 1; y++)
            {
                SetIfNotFixed(ref matrix, ref fixedValues, 0 + y * width, matrix[1 + y * width]);
                SetIfNotFixed(ref matrix, ref fixedValues, (width - 1) + y * width, matrix[(width - 2) + y * width]);
            }
        }
        
        private static void SetIfNotFixed(ref Span<double> values, ref Span<bool> isFixed, int index, double value)
        {
            if (!isFixed[index])
            {
                values[index] = value;
            }
        }
    }
}