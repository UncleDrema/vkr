using Game.Planning;
using NUnit.Framework;
using Unity.Mathematics;

namespace Game.Editor.Tests.Planning
{
    public class UtilsTest
    {
        [TestCase(3, 60, 0, 0, 0, 0)]
        [TestCase(3, 60, 0, 0, 0, 180)]
        [TestCase(3, 60, 1, 0, 0, 90)]
        [TestCase(3, 45, 1, 0, 2, 90)]
        public void Test_PointInSector(float radius, float aperture, float x, float y, float z, float eulerY)
        {
            // Arrange
            var position = new float3(0, 0, 0);
            var rotation = quaternion.Euler(0, math.radians(eulerY), 0);
            var pointInside = new float3(x, y, z);

            // Act
            bool isInside = PlanningUtils.PointInSector(position, rotation, radius, aperture, pointInside);

            // Assert
            Assert.IsTrue(isInside);
        }
        
        [TestCase(3, 60, -1, 0, 0, 0)]
        [TestCase(3, 45, 1, 0, 2, 90)]
        public void Test_PointNotInSector(float radius, float aperture, float x, float y, float z, float eulerY)
        {
            // Arrange
            var position = new float3(0, 0, 0);
            var rotation = quaternion.Euler(0, math.radians(eulerY), 0);
            var pointOutside = new float3(x, y, z);

            // Act
            bool isInside = PlanningUtils.PointInSector(position, rotation, radius, aperture, pointOutside);

            // Assert
            Assert.IsFalse(isInside);
        }
    }
}