using System.Collections.Generic;
using Game.PotentialField.Requests;

namespace Game.SimulationControl
{
    public class SimulationService
    {
        public SimulationMode CurrentSimulationMode { get; set; } = SimulationMode.PotentialFieldMovement;
        public List<AgentDescription> CurrentAgents { get; set; } = new List<AgentDescription>();
    }
}