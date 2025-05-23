using System.Collections.Generic;
using Game.MapGraph.Components;
using Game.Planning.Components;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Transform.Components;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class GameUi : MonoBehaviour
    {
        public TMP_Text zonesThreatText;
        public TMP_Text agentsZonesText;
        
        private World _world;
        private Filter _zones;
        
        public void InjectWorld(World world)
        {
            _world = world;
            _zones = _world.Filter
                .With<ZoneComponent>()
                .Build();
        }

        public void UpdateUi(float deltaTime)
        {
            UpdateZonesThreatInfo();
            UpdateAgentsZonesInfo();
        }

        private void UpdateAgentsZonesInfo()
        {
            var sb = new System.Text.StringBuilder();

            var agents = new List<Entity>();
            foreach (var agent in _world.Filter.With<AgentPatrolComponent>().Build())
                agents.Add(agent);

            agents.Sort((a, b) => a.GetHashCode().CompareTo(b.GetHashCode()));
            foreach (var agent in agents)
            {
                ref var cAgent = ref agent.GetComponent<AgentPatrolComponent>();
                if (cAgent.Zones == null)
                    continue;
                
                sb.AppendLine($"Agent {agent.GetHashCode()}: Zones = {cAgent.Zones.Count}");
                
                foreach (var zone in cAgent.Zones)
                {
                    ref var cZone = ref zone.GetComponent<ZoneComponent>();
                    sb.AppendLine($"  Zone {cZone.ZoneId} ({zone.GetHashCode()})");
                }
            }
            agentsZonesText.text = sb.ToString();
        }

        private void UpdateZonesThreatInfo()
        {
            var sb = new System.Text.StringBuilder();
            var totalThreat = 0f;
            var totalVertices = 0;
            foreach (var zone in _zones)
            {
                ref var cZone = ref zone.GetComponent<ZoneComponent>();
                var zoneThreat = 0f;

                foreach (var vertex in cZone.Vertices)
                {
                    ref var cVertex = ref vertex.GetComponent<GraphVertexComponent>();
                    zoneThreat += cVertex.Threat;
                    totalThreat += cVertex.Threat;
                }
                
                var verticesCount = cZone.Vertices.Count;
                totalVertices += verticesCount;
                sb.AppendLine($"Zone {cZone.ZoneId} ({zone.GetHashCode()}): Threat = {zoneThreat:F2}, Vertices = {verticesCount}, Mean = {zoneThreat / verticesCount:F2}");
            }
            if (totalVertices > 0)
            {
                var meanThreat = totalThreat / totalVertices;
                sb.AppendLine($"Mean Threat: {meanThreat:F2}");
            }
            else
            {
                sb.AppendLine("No vertices found.");
            }
            zonesThreatText.text = sb.ToString();
        }
    }
}