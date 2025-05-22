using System.Collections.Generic;
using System.Text;
using Game.MapGraph.Components;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Game.MapGraph.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ZoneDriftSystem : UpdateSystem
    {
        private Filter _zones;
        private Filter _vertices;
        
        public override void OnAwake()
        {
            _zones = World.Filter
                .With<ZoneComponent>()
                .Build();
            _vertices = World.Filter
                .With<GraphVertexComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            // 1) Считаем суммарную и среднюю угрозу для каждой зоны
            var zoneList = new NativeList<Entity>(Allocator.Temp);
            var zoneTotal = new Dictionary<Entity, float>();
            var zoneCount = new Dictionary<Entity, int>();
            int totalVertices = 0;
            int totalZones = 0;
            foreach (var zone in _zones)
            {
                zoneList.Add(zone);
                ref var cZone = ref zone.GetComponent<ZoneComponent>();

                float total = 0f;
                int count = 0;
                foreach (var vertex in cZone.Vertices)
                {
                    ref var cVertex = ref vertex.GetComponent<GraphVertexComponent>();
                    total += cVertex.Threat;
                    count++;
                }
                
                zoneTotal[zone] = total;
                zoneCount[zone] = count;
                totalVertices += count;
                totalZones++;
            }
            
            // 2) Собираем кандидатов (граничные вершины) и их выгоду Δ
            var candidates = new List<(Entity v, Entity fromZone, Entity toZone, float delta)>();

            foreach (var vertex in _vertices)
            {
                ref var cVertex = ref vertex.GetComponent<GraphVertexComponent>();
                float Uv = cVertex.Threat;
                var zone = cVertex.Zone;
                var neighbors = cVertex.Neighbors;
                
                var neighborZones = new HashSet<Entity>();
                foreach (var neighbor in neighbors)
                {
                    ref var cNeighbor = ref neighbor.GetComponent<GraphVertexComponent>();
                    if (cNeighbor.Zone != zone && cNeighbor.Zone != default)
                    {
                        neighborZones.Add(cNeighbor.Zone);
                    }
                }
                
                // i-zone: zone, j-zone: toZone
                // для каждой смежной зоны вычисляем Δ
                const float lambdaT = 1f;
                const float lambdaS = 10f;
                foreach (var toZone in neighborZones)
                {
                    float Ti = zoneTotal[zone];
                    float Tj = zoneTotal[toZone];
                    float deltaT = (Ti - Uv) - (Tj + Uv);
                    float deltaS = zoneCount[zone] - zoneCount[toZone] - 2;
                    float delta = lambdaT * deltaT + lambdaS * deltaS;
                    if (delta > 0)
                    {
                        candidates.Add((vertex, zone, toZone, delta));
                    }
                }
            }
            
            // 3) Сортируем кандидатов по убыванию выгоды Δ
            candidates.Sort((a, b) => b.delta.CompareTo(a.delta));
            
            // 4) Пытаемся выполнить первый допустимый перенос
            foreach (var (v, fromZone, toZone, delta) in candidates)
            {
                // Проверяем, что без вершины v зона fromZone останется связной
                if (!WouldRemainConnected(fromZone, v))
                    continue;
                
                ref var zoneFrom = ref fromZone.GetComponent<ZoneComponent>();
                ref var zoneTo = ref toZone.GetComponent<ZoneComponent>();
                ref var cVertex = ref v.GetComponent<GraphVertexComponent>();
                
                zoneFrom.Vertices.Remove(v);
                zoneTo.Vertices.Add(v);
                cVertex.Zone = toZone;
            }

            zoneList.Dispose();
        }

        private bool WouldRemainConnected(Entity zone, Entity removingVertex)
        {
            ref var cZone = ref zone.GetComponent<ZoneComponent>();
            var verts = cZone.Vertices;
            if (verts.Count <= 1) return true;

            // найти стартовую
            Entity start = default;
            foreach (var v in verts)
            {
                if (v != removingVertex) { start = v; break; }
            }
            if (start == default) return true;

            var visited = new HashSet<Entity>();
            var queue   = new Queue<Entity>();
            visited.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                ref var neighs = ref u.GetComponent<GraphVertexComponent>().Neighbors;
                foreach (var n in neighs)
                {
                    if (n == removingVertex) continue;
                    if (!visited.Contains(n) && verts.Contains(n))
                    {
                        visited.Add(n);
                        queue.Enqueue(n);
                    }
                }
            }

            // Если посетили все кроме removeV, связность сохраняется
            return visited.Count == verts.Count - 1;
        }
    }
}