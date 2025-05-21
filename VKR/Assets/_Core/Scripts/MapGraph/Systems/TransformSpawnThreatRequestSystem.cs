using Game.MapGraph.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Game.MapGraph.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TransformSpawnThreatRequestSystem : UpdateSystem
    {
        private Filter _spawnThreatByPositionRequests;
        
        private GraphService _graphService;
        
        public TransformSpawnThreatRequestSystem(GraphService graphService)
        {
            _graphService = graphService;
        }
        
        public override void OnAwake()
        {
            _spawnThreatByPositionRequests = World.Filter
                .With<SpawnThreatByPositionRequest>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var request in _spawnThreatByPositionRequests)
            {
                ref var cSpawnRequest = ref request.GetComponent<SpawnThreatByPositionRequest>();
                
                // Transform the position to the graph space
                var vertex = _graphService.GetNearestVertex(cSpawnRequest.Position);
                if (vertex == default)
                {
                    Debug.LogError($"No vertex found for position {cSpawnRequest.Position}");
                }
                else
                {
                    ref var cNewRequest = ref request.AddComponent<SpawnThreatRequest>();
                    cNewRequest.TargetVertex = vertex;
                    cNewRequest.ThreatLevel = cSpawnRequest.ThreatLevel;
                    cNewRequest.ThreadDuration = cSpawnRequest.ThreadDuration;
                    cNewRequest.DecayType = cSpawnRequest.DecayType;
                }
            }
        }
    }
}