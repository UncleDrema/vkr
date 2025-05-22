using Game.MapGraph.Components;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Game.MapGraph.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UpdateThreatSystem : UpdateSystem
    {
        private Filter _vertices;
        private Filter _threatFilter;
        private Stash<GraphVertexComponent> _verticesStash;
        
        public override void OnAwake()
        {
            _vertices = World.Filter
                .With<GraphVertexComponent>()
                .Build();
            _threatFilter = World.Filter
                .With<ThreatEventComponent>()
                .Build();
            _verticesStash = World.GetStash<GraphVertexComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var vertex in _vertices)
            {
                ref GraphVertexComponent cVertex = ref _verticesStash.Get(vertex);
                var visitedTime = cVertex.LastObservationTime;
                var timeFromLastVisit = Time.time - visitedTime;
                // Сбрасываем угрозу на вершине
                var threatFromUnvisited = 10 * math.tanh(timeFromLastVisit / 120f);
                cVertex.Threat = threatFromUnvisited;
            }
            foreach (var entity in _threatFilter)
            {
                ref var cThreat = ref entity.GetComponent<ThreatEventComponent>();
                
                cThreat.ExistTime += deltaTime;
                
                if (cThreat.ExistTime >= cThreat.Duration)
                {
                    entity.RemoveComponent<ThreatEventComponent>();
                }
                else
                {
                    ref GraphVertexComponent cVertex = ref _verticesStash.Get(cThreat.TargetVertex);
                    cVertex.Threat += ThreatUtils.GetThreatLevel(cThreat.ThreatLevel, cThreat.Duration, cThreat.ExistTime, cThreat.DecayType);
                }
            }
        }
    }
}