using System.Collections.Generic;
using Game.MapGraph.Components;
using Game.Planning.Components;
using Game.PotentialField.Requests;
using Game.SimulationControl.Components;
using Game.SimulationControl.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Feature.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SimulationMode = Game.SimulationControl.SimulationMode;

namespace Game.UI
{
    public class GameUi : MonoBehaviour
    {
        public TMP_Text zonesThreatText;
        public TMP_Text agentsZonesText;
        public TMP_Text timeFpsText;
        public Button startPotentialFieldsButton;
        public Button startSimpleButton;
        public Button collectStatisticsButton;
        
        public AgentRow agentRowPrefab;
        
        public GameObject agentRowContainer;
        
        public Button agentsPlusButton;
        public Button agentsMinusButton;
        
        private World _world;
        private Filter _zones;
        
        private void OnEnable()
        {
            startPotentialFieldsButton.onClick.AddListener(OnStartPotentialFieldsButtonClicked);
            startSimpleButton.onClick.AddListener(OnStartSimpleButtonClicked);
            collectStatisticsButton.onClick.AddListener(OnCollectStatisticsButtonClicked);
            agentsPlusButton.onClick.AddListener(OnAgentsPlusButtonClicked);
            agentsMinusButton.onClick.AddListener(OnAgentsMinusButtonClicked);
        }

        private void OnDisable()
        {
            startPotentialFieldsButton.onClick.RemoveListener(OnStartPotentialFieldsButtonClicked);
            startSimpleButton.onClick.RemoveListener(OnStartSimpleButtonClicked);
            collectStatisticsButton.onClick.RemoveListener(OnCollectStatisticsButtonClicked);
            agentsPlusButton.onClick.RemoveListener(OnAgentsPlusButtonClicked);
            agentsMinusButton.onClick.RemoveListener(OnAgentsMinusButtonClicked);
        }

        public void SetAgents(List<AgentDescription> agents)
        {
            foreach (Transform child in agentRowContainer.transform)
            {
                if (child.GetComponentInChildren<AgentRow>() != null)
                    Destroy(child.gameObject);
            }

            foreach (var agent in agents)
            {
                var agentRow = Instantiate(agentRowPrefab, agentRowContainer.transform);
                agentRow.FieldX.text = agent.Position.x.ToString();
                agentRow.FieldY.text = agent.Position.y.ToString();
                agentRow.FieldZ.text = agent.Position.z.ToString();
                agentRow.FieldSpeed.text = agent.Speed.ToString();
            }
        }
        
        public List<AgentDescription> GetAgents()
        {
            var result = new List<AgentDescription>();

            var rows = agentRowContainer.GetComponentsInChildren<AgentRow>();
            foreach (var row in rows)
            {
                try
                {
                    var agent = new AgentDescription
                    {
                        Position = new Vector3(
                            float.Parse(row.FieldX.text),
                            float.Parse(row.FieldY.text),
                            float.Parse(row.FieldZ.text)),
                        Speed = float.Parse(row.FieldSpeed.text)
                    };
                    result.Add(agent);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing agent data: {e.Message}");
                }
            }

            return result;
        }
        
        private void OnAgentsMinusButtonClicked()
        {
            if (agentRowContainer.transform.childCount > 1)
            {
                Destroy(agentRowContainer.transform.GetChild(agentRowContainer.transform.childCount - 1).gameObject);
            }
        }
        
        private void OnAgentsPlusButtonClicked()
        {
            var agentRow = Instantiate(agentRowPrefab, agentRowContainer.transform);
            agentRow.FieldX.text = "0";
            agentRow.FieldY.text = "0";
            agentRow.FieldZ.text = "0";
            agentRow.FieldSpeed.text = "1";
        }
        
        private void OnStartSimpleButtonClicked()
        {
            ref var cReq = ref _world.CreateEventEntity<StartSimulationRequest>();
            cReq.Mode = SimulationMode.SimpleMovement;
            cReq.Agents = GetAgents();
        }

        private void OnStartPotentialFieldsButtonClicked()
        {
            ref var cReq = ref _world.CreateEventEntity<StartSimulationRequest>();
            cReq.Mode = SimulationMode.PotentialFieldMovement;
            cReq.Agents = GetAgents();
        }
        
        private void OnCollectStatisticsButtonClicked()
        {
            ref var cReq = ref _world.CreateEventEntity<CollectStatisticsRequest>();
        }
        
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
            UpdateTimeFpsInfo(deltaTime);
        }

        private void UpdateTimeFpsInfo(float deltaTime)
        {
            foreach (var simulation in _world.Filter.With<SimulationComponent>().Build())
            {
                ref var cSimulation = ref simulation.GetComponent<SimulationComponent>();
                var passedTime = cSimulation.TimePassed;
                var fps = 1f / deltaTime;
                
                timeFpsText.text = $"Время: {passedTime:F2}сек.\nFPS: {fps:F2}";
            }
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