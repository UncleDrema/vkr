using Unity.Mathematics;
using UnityEngine;

namespace Game.Planning
{
    public static class PlanningUtils
    {
        public static bool PointInRadius(float3 position, float radius, float3 point)
        {
            // Проверим, что точка находится в пределах радиуса
            return math.distance(position, point) <= radius;
        }
        
        public static bool PointInSector(float3 position, quaternion rotation, float radius, float aperture, float3 point)
        {
            if (position.Equals(point))
                return true;
            
            // Проверим, что точка находится в пределах радиуса
            if (math.distance(position, point) > radius)
                return false;
            
            // Проверим, что точка находится в пределах угла aperture, при повороте rotation
            var direction = math.normalize(point - position);
            var forward = math.forward(rotation);
            float dotProduct = math.dot(forward, direction);
            return dotProduct >= math.cos(aperture / 2f);
        }

        public static bool LineIntersectsBounds(float3 position, float3 vertexPosition, Bounds bounds)
        {
            // Проверим пересечение линии между позицией агента и вершиной с границами препятствия
            var lineDirection = math.normalize(vertexPosition - position);
            var lineLength = math.distance(position, vertexPosition);
            var lineEnd = position + lineDirection * lineLength;
            var line = new Ray(position, lineDirection);
            // Проверим пересечение линии с границами
            if (bounds.IntersectRay(line, out var distance))
            {
                // Проверим, что точка пересечения находится на линии между позицией агента и вершиной
                if (distance < lineLength)
                {
                    return true;
                }
            }
            // Проверим, что точка пересечения находится на границах
            if (bounds.Contains(lineEnd))
            {
                return true;
            }
            // Проверим, что точка пересечения находится на границах
            if (bounds.Contains(position))
            {
                return true;
            }
            return false;
        }
    }
}