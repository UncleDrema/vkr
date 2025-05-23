using System.Collections.Generic;
using System.Linq;
using Game.MapGraph.Components;
using Game.MapGraph.Events;
using Game.Planning.Components;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Transform.Components;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Planning.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ZoneAssignmentSystem : UpdateSystem
    {
        private Filter _zonesInitializedEvents;
        private Filter _agents;
        private Filter _zones;
        
        public override void OnAwake()
        {
            _zonesInitializedEvents = World.Filter
                .With<ZonesInitializedEvent>()
                .Build();
            
            _agents = World.Filter
                .With<AgentPatrolComponent>()
                .With<TransformComponent>()
                .Build();

            _zones = World.Filter
                .With<ZoneComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_zonesInitializedEvents.IsEmpty())
                return;
            
            var zones = new List<Entity>();
            var agents = new List<Entity>();
            foreach (var zone in _zones)
            {
                zones.Add(zone);
            }
            foreach (var agent in _agents)
            {
                agents.Add(agent);
            }

            int Z = zones.Count;
            int A = agents.Count;
            
            // Предвычислим для каждой зоны её «центр» и суммарную опасность
            var zoneCenter = new Dictionary<Entity, float3>();
            var zoneThreat = new Dictionary<Entity, float>();
            foreach (var z in zones)
            {
                var vertices = z.GetComponent<ZoneComponent>().Vertices;
                float3 sumPos = float3.zero;
                float sumThreat = 0;
                foreach (var vertex in vertices)
                {
                    ref var cVertex = ref vertex.GetComponent<GraphVertexComponent>();
                    sumPos += cVertex.Position;
                    sumThreat += cVertex.Threat;
                }
                zoneCenter[z] = sumPos / vertices.Count;
                zoneThreat[z] = sumThreat / vertices.Count;
            }
            
            // Стоимости C[k,i] = α·d(agent_k,center_i) - β·zoneDanger[i]
            const float alphaDistance = 1;
            const float betaDanger = 10;
            // Жадно для каждого агента в порядке возрастания стоимости назначаем зону
            var assignments = new Dictionary<Entity, List<Entity>>();
            var assignedZones = new HashSet<Entity>();
            
            var costList = new List<(Entity agent, Entity zone, float cost)>();
            foreach (var a in agents)
            {
                ref var cTransform = ref a.GetComponent<TransformComponent>();
                var agentPos = cTransform.Position();
                assignments[a] = new List<Entity>();
                foreach (var z in zones)
                {
                    var zonePos = zoneCenter[z];
                    var zoneDanger = zoneThreat[z];
                    var distance = math.distance(agentPos, zonePos);
                    var cost = alphaDistance * distance - betaDanger * zoneDanger;
                    
                    costList.Add((a, z, cost));
                }
            }
            
            // Сортируем по возрастанию стоимости
            costList.Sort((x, y) => x.cost.CompareTo(y.cost));
            
            // Жадно назначаем, пока каждому агенту не дастся хотя бы одна зона
            foreach (var (agent, zone, cost) in costList)
            {
                if (assignments[agent].Count == 0 && !assignedZones.Contains(zone))
                {
                    Debug.Log($"Assigning zone {zone.GetHashCode()} to agent {agent.GetHashCode()} with cost {cost}");
                    assignments[agent].Add(zone);
                    assignedZones.Add(zone);
                }
            }
            
            // Если остались незанятые зоны, назначаем их агентам с минимальной стоимостью
            foreach (var (agent, zone, cost) in costList)
            {
                if (!assignedZones.Contains(zone))
                {
                    Debug.Log($"Zone {zone.GetHashCode()} is unassigned, assigning to agent {agent.GetHashCode()} with cost {cost}");
                    assignments[agent].Add(zone);
                    assignedZones.Add(zone);
                    if (assignments.Count == A)
                    {
                        break;
                    }
                }
            }
            
            // Присваиваем зоны агентам
            foreach (var agent in agents)
            {
                ref var cPatrol = ref agent.GetComponent<AgentPatrolComponent>();
                cPatrol.Zones = assignments[agent];
                var zonesString = string.Join(", ", assignments[agent].Select(z =>
                {
                    ref var cZone = ref z.GetComponent<ZoneComponent>();
                    return $"{cZone.ZoneId}";
                }));
                Debug.Log($"Assigned zones {zonesString} to agent {agent}");
            }
        }
    }
}