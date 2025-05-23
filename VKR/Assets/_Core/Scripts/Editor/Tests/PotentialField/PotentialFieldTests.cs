using Game.PotentialField.Components;
using Game.PotentialField.Events;
using Game.PotentialField.Requests;
using NUnit.Framework;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Transform.Components;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Editor.Tests.PotentialField
{
    public class PotentialFieldTests
    {
        private float3 GetRandomPositionInWorld(int width, int height, float cellSize)
        {
            var x = UnityEngine.Random.Range(1, width - 1) * cellSize - width * cellSize / 2;
            var y = 0f;
            var z = UnityEngine.Random.Range(1, height - 1) * cellSize - height * cellSize / 2;
            return new float3(x, y, z);
        }
        
        /*
        [TestCase(10, 10, 1, 10, 10)]
        [TestCase(25, 25, 1, 10, 100)]
        [TestCase(50, 50, 1, 10, 250)]
        public void Test_GoalReachedWithoutObstacles(int width, int height, float cellSize, int repeats, float simulationTime)
        {
            for (int i = 0; i < repeats; i++)
            {
                var world = MethodTestUtils.CreateWorld();
            
                var map = MethodTestUtils.CreateMap(world, float3.zero, width, height, cellSize);
                var agent = MethodTestUtils.CreateAgent(world, GetRandomPositionInWorld(width, height, cellSize), 1);

                MethodTestUtils.SetAgentGoal(agent, GetRandomPositionInWorld(width, height, cellSize));
            
                bool goalReached = false;
                float passedTime = 0f;
                while (passedTime < simulationTime)
                {
                    ref var cMapPos = ref agent.GetComponent<MapPositionComponent>();
                    ref var cTransform = ref agent.GetComponent<TransformComponent>();
                    if (world.Any((ref GoalReachedEvent component) => true))
                    {
                        Debug.Log($"Goal reached in {passedTime:F2} seconds");
                        goalReached = true;
                        break;
                    }
                    world.Update60Fps();
                    passedTime += EcsTestUtils.DeltaTime60Fps;
                }
            
                Assert.IsTrue(goalReached, "Goal was not reached within the simulation time.");
            }
        }

        [Test]
        public void Test_GoalReachedWithObstacleOnWay()
        {
            int width = 20;
            int height = 20;
            float cellSize = 1f;
            float simulationTime = 50f;
            var world = MethodTestUtils.CreateWorld();
            
            var map = MethodTestUtils.CreateMap(world, float3.zero, width, height, cellSize);
            var agent = MethodTestUtils.CreateAgent(world, float3.zero, 1);
            var obstacle = MethodTestUtils.CreateObstacle(world, new float3(4, 0, 0), new float2(0.5f, 1));
            MethodTestUtils.SetAgentGoal(agent, new float3(8, 0, 0));
            
            bool goalReached = false;
            float passedTime = 0f;
            while (passedTime < simulationTime)
            {
                ref var cMapPos = ref agent.GetComponent<MapPositionComponent>();
                ref var cTransform = ref agent.GetComponent<TransformComponent>();
                if (world.Any((ref GoalReachedEvent component) => true))
                {
                    Debug.Log($"Goal reached in {passedTime:F2} seconds");
                    goalReached = true;
                    break;
                }
                world.Update60Fps();
                passedTime += EcsTestUtils.DeltaTime60Fps;
            }
            
            Assert.IsTrue(goalReached, "Goal was not reached within the simulation time.");
        }
        */
        
        [Test]
        public void Test_CantSetGoalWhenSurroundedByObstacles()
        {
            int width = 20;
            int height = 20;
            float cellSize = 1f;
            float simulationTime = 1f;
            var world = MethodTestUtils.CreateWorld();
            
            var map = MethodTestUtils.CreateMap(world, float3.zero, width, height, cellSize);
            var agent = MethodTestUtils.CreateAgent(world, float3.zero, 1);
            var obstacle1 = MethodTestUtils.CreateObstacle(world, new float3(0, 0, 1), new float2(1.5f, 0.3f));
            var obstacle2 = MethodTestUtils.CreateObstacle(world, new float3(0, 0, -1), new float2(1.5f, 0.3f));
            var obstacle3 = MethodTestUtils.CreateObstacle(world, new float3(1, 0, 0), new float2(0.3f, 1.5f));
            var obstacle4 = MethodTestUtils.CreateObstacle(world, new float3(-1, 0, 0), new float2(0.3f, 1.5f));
            MethodTestUtils.SetAgentGoal(agent, new float3(8, 0, 0));
            
            bool simulationGoalReached = false;
            float passedTime = 0f;
            while (passedTime < simulationTime)
            {
                ref var cMapPos = ref agent.GetComponent<MapPositionComponent>();
                ref var cTransform = ref agent.GetComponent<TransformComponent>();
                if (world.Any((ref SetGoalFailEvent component) => true))
                {
                    Debug.Log($"Set goal failed in {passedTime:F2} seconds");
                    simulationGoalReached = true;
                    break;
                }
                world.Update60Fps();
                passedTime += EcsTestUtils.DeltaTime60Fps;
            }
            
            Assert.IsTrue(simulationGoalReached, "Goal was set when it should not have been.");
        }
    }
}