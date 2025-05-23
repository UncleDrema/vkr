using Game.MapGraph.Components;
using Game.SimulationControl.Components;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Game.SimulationControl.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UpdateSimulationSystem : UpdateSystem
    {
        private Filter _simulation;
        private Filter _zones;
        
        public override void OnAwake()
        {
            _simulation = World.Filter
                .With<SimulationComponent>()
                .Build();
            _zones = World.Filter
                .With<ZoneComponent>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var simulation in _simulation)
            {
                ref var cSimulation = ref simulation.GetComponent<SimulationComponent>();
                
                var totalThreat = 0f;
                var totalVertices = 0;
                foreach (var zone in _zones)
                {
                    ref var cZone = ref zone.GetComponent<ZoneComponent>();
                    
                    var zoneThreat = 0f;
                    var zoneVertices = 0;
                    foreach (var vertex in cZone.Vertices)
                    {
                        ref var cVertex = ref vertex.GetComponent<GraphVertexComponent>();
                        zoneThreat += cVertex.Threat;
                        zoneVertices++;
                    }
                    
                    Debug.Log($"Zone {cZone.ZoneId} threat: {zoneThreat}, vertices: {zoneVertices}");
                    cSimulation.AddEvent($"zone{cZone.ZoneId}", new ZoneEvent()
                    {
                        Threat = zoneThreat,
                        Vertices = zoneVertices
                    });
                    
                    totalThreat += zoneThreat;
                    totalVertices += zoneVertices;
                }
                
                cSimulation.AddEvent("zoneTotal", new ZoneEvent()
                {
                    Threat = totalThreat,
                    Vertices = totalVertices
                });
                
                Debug.Log($"Simulation time: {cSimulation.TimePassed}");
                cSimulation.TimePassed += deltaTime;
            }
        }
        
        private class ZoneEvent
        {
            public float Threat;
            public int Vertices;
        }
    }
}