using System;
using System.Collections.Generic;
using Game.MapGraph.Components;
using Game.Planning.Components;
using Game.PotentialField;
using Game.PotentialField.Components;
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
        public Canvas GameUiCanvas;
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

        public GameObject errorPanel;
        public TMP_Text errorText;
        
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                GameUiCanvas.enabled = !GameUiCanvas.enabled;
            }
        }

        public void ShowError(string error)
        {
            errorPanel.SetActive(true);
            errorText.text = error;
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
        
        public List<AgentDescription> GetAgents(out bool valid)
        {
            var result = new List<AgentDescription>();
            valid = true;

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
                    valid = false;
                    Debug.LogError($"Error parsing agent data: {e.Message}");
                    break;
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
            StartSimulationWithAgentsFromTable(SimulationMode.SimpleMovement);
        }

        private void OnStartPotentialFieldsButtonClicked()
        {
            StartSimulationWithAgentsFromTable(SimulationMode.PotentialFieldMovement);
        }
        
        private void StartSimulationWithAgentsFromTable(SimulationMode mode)
        {
            var agents = GetAgents(out var valid);
            
            if (!valid)
            {
                ShowError("Некорректные параметры агентов.");
                return;
            }
            
            if (agents.Count == 0)
            {
                ShowError("Не указаны агенты.");
                return;
            }
            var vertexCount = _world.Filter
                .With<GraphVertexComponent>()
                .Build()
                .GetLengthSlow();
            if (agents.Count > vertexCount)
            {
                ShowError($"Количество агентов ({agents.Count}) больше количества вершин ({vertexCount}).");
                return;
            }
            
            // Проверим что у всех агентов позиция в пределах карты и скорость больше нуля
            for (int i = 0; i < agents.Count; i++)
            {
                var agent = agents[i];
                if (agent.Speed <= 0)
                {
                    ShowError($"У агента {i} скорость должна быть больше нуля.");
                    return;
                }
                
                var ms = new MapService();
                ms.Initialize(_world);
                var mapPos = ms.WorldToMapPosition(agent.Position);
                foreach (var map in _world.Filter.With<GlobalMapComponent>().Build())
                {
                    ref var cMap = ref map.GetComponent<GlobalMapComponent>();
                    if (mapPos.x < 0 || mapPos.x >= cMap.Width || mapPos.y < 0 || mapPos.y >= cMap.Height)
                    {
                        ShowError($"Позиция агента {i} ({agent.Position}) выходит за пределы карты.");
                        return;
                    }
                }
            }

            ref var cReq = ref _world.CreateEventEntity<StartSimulationRequest>();
            cReq.Mode = mode;
            cReq.Agents = agents;
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