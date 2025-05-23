using Game.SimulationControl.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Feature.Events;
using TriInspector;
using UnityEngine;

namespace Game.SimulationControl
{
    public class SimulationHelper : MonoBehaviour
    {
        [Button]
        public void StartSimulation(SimulationMode mode)
        {
            ref var cStartReq = ref World.Default.CreateEventEntity<StartSimulationRequest>();
            cStartReq.Mode = mode;
        }
    }
}