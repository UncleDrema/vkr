using System.Collections.Generic;
using System.Linq;
using Game.MapGraph;
using Game.MapGraph.Components;
using Game.MapGraph.Events;
using NUnit.Framework;
using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Editor.Tests.MapGraph
{
    public class MapGraphTests
    {
        [Test]
        public void Test_GraphCreatesRequiredComponents()
        {
            // Arrange
            var world = MethodTestUtils.CreateWorld();

            MethodTestUtils.CreateGraph(world, new List<Vector3>()
                {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 1, 0),
                    new(1, 1, 0)
                },
                new List<Edge>()
                {
                    new(0, 1),
                    new(1, 3),
                    new(3, 2),
                    new(2, 0)
                },
                1f);
            
            // Act
            world.Update60Fps();
            
            // Assert
            Assert.That(world.CountOf<GraphVertexComponent>() == 4);
            Assert.That(world.CountOf<GraphComponent>() == 1);
            Assert.That(world.All((ref GraphComponent graph) => graph.Vertices.Count == 4 && graph.Edges.Count == 4));
        }
        
        [Test]
        public void Test_SmallSegmentLength_ResultsInMoreVerticesAndEdges()
        {
            // Arrange
            var world = MethodTestUtils.CreateWorld();

            MethodTestUtils.CreateGraph(world, new List<Vector3>()
                {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 1, 0),
                    new(1, 1, 0)
                },
                new List<Edge>()
                {
                    new(0, 1),
                    new(1, 3),
                    new(3, 2),
                    new(2, 0)
                },
                0.5f);
            
            // Act
            world.Update60Fps();
            
            // Assert
            Assert.That(world.CountOf<GraphVertexComponent>() == 8);
            Assert.That(world.CountOf<GraphComponent>() == 1);
            Assert.That(world.All((ref GraphComponent graph) => graph.Vertices.Count == 8 && graph.Edges.Count == 8));
        }

        [TestCase(10, 0.15f, 1)]
        [TestCase(10, 0.15f, 2)]
        [TestCase(10, 0.15f, 3)]
        [TestCase(10, 0.15f, 4)]
        [TestCase(10, 0.15f, 5)]
        public void Test_ZonesAreCreated(int basicVerticesCount, float density, int zoneCount)
        {
            // Arrange
            var vertices = new List<Vector3>();
            var edges = new List<Edge>();
            for (int i = 0; i < basicVerticesCount; i++)
            {
                var randomX = Random.Range(-10f, 10f);
                var randomZ = Random.Range(-10f, 10f);
                vertices.Add(new Vector3(randomX, 0f, randomZ));
            }
            
            for (int i = 0; i < basicVerticesCount; i++)
            {
                for (int j = i + 1; j < basicVerticesCount; j++)
                {
                    edges.Add(new Edge(i, j));
                }
            }
            
            for (int i = 1; i < basicVerticesCount - 1; i++)
            {
                for (int j = i + 1; j < basicVerticesCount; j++)
                {
                    if (Random.value < density)
                    {
                        edges.Add(new Edge(i, j));
                    }
                }
            }
            
            var world = MethodTestUtils.CreateWorld();
            var graph = MethodTestUtils.CreateGraph(world, vertices, edges, 1f);
            var zones = MethodTestUtils.CreateZones(world, zoneCount);
            
            // Act
            world.Update60Fps();
            
            // Assert
            Assert.That(world.CountOf<ZoneComponent>() == zoneCount);
            world.ForEach((ref ZoneComponent zone) =>
            {
                Debug.Log($"ZoneId: {zone.ZoneId}, VerticesCount: {zone.Vertices.Count}");
                Assert.That(zone.Vertices.Count > 0);
                Assert.That(zone.ZoneId >= 0 && zone.ZoneId < zoneCount);
            });
            world.ForEach((ref GraphVertexComponent vertex) =>
            {
                Assert.That(vertex.Zone != default);
            });
            Assert.That(world.CountOf<ZonesInitializedEvent>() > 0);
        }
        
        [Test]
        public void Test_ThreatSpawnsInTargetAndNeighborVertices()
        {
            // Arrange
            var world = MethodTestUtils.CreateWorld();
            var graph = MethodTestUtils.CreateGraph(world, new List<Vector3>()
                {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                    new(-1, 0, 0),
                    new(0, 0, -1),
                    new(1, 0, 1),
                },
                new List<Edge>()
                {
                    new(0, 1),
                    new(1, 2),
                    new(2, 3),
                    new(3, 0),
                    new(3, 4),
                    new(2, 5),
                },
                10f);
            
            MethodTestUtils.CreateThreat(world, new float3(0, 0, 0), 5f, 1f, ThreatDecayType.DecayAfter50Percent);
            
            // Act
            world.Update60Fps();
            
            // Assert
            // Число угроз равно целевой вершине + число соседей
            Assert.That(world.CountOf<ThreatEventComponent>() == 3);
            // Уровень угрозы в целевой вершине почти равен 5, в соседних меньше, но больше среднего уровня угрозы по остальным вершинам
            var notNeighborCount = 0;
            var notNeighborThreatSum = 0f;
            world.ForEach((ref GraphVertexComponent vertex) =>
            {
                if (vertex.Position.Equals(new float3(0, 0, 0)) || vertex.Neighbors.Any(v =>
                    {
                        ref var cVertex = ref v.GetComponent<GraphVertexComponent>();
                        return cVertex.Position.Equals(new float3(0, 0, 0));
                    })) return;
                notNeighborCount++;
                notNeighborThreatSum += vertex.Threat;
            });
            
            var notNeighborAverage = notNeighborThreatSum / notNeighborCount;
            world.ForEach((ref GraphVertexComponent vertex) =>
            {
                if (vertex.Position.Equals(new float3(0, 0, 0)))
                {
                    Assert.That(vertex.Threat > 4.5f);
                }
                // У соседей уровень угрозы больше среднего уровня угрозы среди всех остальных вершин
                else if (vertex.Neighbors.Any(v =>
                         {
                             ref var cVertex = ref v.GetComponent<GraphVertexComponent>();
                             return cVertex.Position.Equals(new float3(0, 0, 0));
                         }))
                {
                    Assert.That(vertex.Threat > notNeighborAverage);
                }
            });
        }

        [Test]
        public void Test_ZoneDrift()
        {
            // Arrange
            var world = MethodTestUtils.CreateWorld();
            var graph = MethodTestUtils.CreateGraph(world, new List<Vector3>()
                {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 1, 0),
                    new(1, 1, 0)
                },
                new List<Edge>()
                {
                    new(0, 1),
                    new(1, 3),
                    new(3, 2),
                    new(2, 0)
                },
                0.1f);
            
            var zones = MethodTestUtils.CreateZones(world, 2);
            world.Update60Fps();
            
            // Создадим очень сильную угрозу в зоне 0 в её первой вершине
            Entity zone0 = default;
            Entity zone1 = default;
            world.ForEach((Entity e, ref ZoneComponent zone) =>
            {
                if (zone.ZoneId == 0)
                {
                    zone0 = e;
                }
                else if (zone.ZoneId == 1)
                {
                    zone1 = e;
                }
            });
            
            ref var cZone0 = ref zone0.GetComponent<ZoneComponent>();
            ref var cZone1 = ref zone1.GetComponent<ZoneComponent>();
            ref var firstVertex = ref cZone0.Vertices[0].GetComponent<GraphVertexComponent>();
            
            MethodTestUtils.CreateThreat(world, firstVertex.Position, 100f, 1f, ThreatDecayType.DecayAfter75Percent);
            
            // Запомним размеры зон
            var zone0Size = cZone0.Vertices.Count;
            var zone1Size = cZone1.Vertices.Count;
            
            // Act
            world.Update60Fps(0.5f);
            
            // Assert
            // Зона 0 должна уменьшиться, а зона 1 увеличиться
            world.ForEach((ref ZoneComponent zone) =>
            {
                if (zone.ZoneId == 0)
                {
                    Assert.That(zone.Vertices.Count < zone0Size);
                }
                else if (zone.ZoneId == 1)
                {
                    Assert.That(zone.Vertices.Count > zone1Size);
                }
            });
        }

        [Test]
        public void Test_ZoneDrift_AndBack()
        {
            // Проверим, что если угроза исчезает, то зоны возвращаются обратно
            // Arrange
            var world = MethodTestUtils.CreateWorld();
            var graph = MethodTestUtils.CreateGraph(world, new List<Vector3>()
                {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 1, 0),
                    new(1, 1, 0)
                },
                new List<Edge>()
                {
                    new(0, 1),
                    new(1, 3),
                    new(3, 2),
                    new(2, 0)
                },
                0.1f);
            
            var zones = MethodTestUtils.CreateZones(world, 2);
            world.Update(0.5f);
            
            // Создадим очень сильную угрозу в зоне 0 в её первой вершине
            Entity zone0 = default;
            Entity zone1 = default;
            world.ForEach((Entity e, ref ZoneComponent zone) =>
            {
                if (zone.ZoneId == 0)
                {
                    zone0 = e;
                }
                else if (zone.ZoneId == 1)
                {
                    zone1 = e;
                }
            });
            
            ref var cZone0 = ref zone0.GetComponent<ZoneComponent>();
            ref var cZone1 = ref zone1.GetComponent<ZoneComponent>();
            ref var firstVertex = ref cZone0.Vertices[0].GetComponent<GraphVertexComponent>();
            
            MethodTestUtils.CreateThreat(world, firstVertex.Position, 100f, 1f, ThreatDecayType.DecayAfter75Percent);
            
            // Запомним размеры зон
            var zone0Size = cZone0.Vertices.Count;
            var zone1Size = cZone1.Vertices.Count;
            
            world.Update60Fps(0.5f);
            
            // Зона 0 должна уменьшиться, а зона 1 увеличиться
            world.ForEach((ref ZoneComponent zone) =>
            {
                if (zone.ZoneId == 0)
                {
                    Assert.That(zone.Vertices.Count < zone0Size);
                    zone0Size = zone.Vertices.Count;
                }
                else if (zone.ZoneId == 1)
                {
                    Assert.That(zone.Vertices.Count > zone1Size);
                    zone1Size = zone.Vertices.Count;
                }
            });
            
            // Act
            // Подождем, пока угроза исчезнет
            world.Update60Fps(1.5f);
            
            // Assert
            // Зона 0 должна стать опять больше, а зона 1 меньше
            world.ForEach((ref ZoneComponent zone) =>
            {
                if (zone.ZoneId == 0)
                {
                    Assert.That(zone.Vertices.Count > zone0Size);
                }
                else if (zone.ZoneId == 1)
                {
                    Assert.That(zone.Vertices.Count < zone1Size);
                }
            });
        }
    }
}