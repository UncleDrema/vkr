using Game.MapGraph.Components;
using Game.MapGraph.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;

namespace Game.MapGraph.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SpawnThreatSystem : UpdateSystem
    {
        private Filter _spawnThreatRequests;
        
        public override void OnAwake()
        {
            _spawnThreatRequests = World.Filter
                .With<SpawnThreatRequest>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var request in _spawnThreatRequests)
            {
                ref var cSpawnRequest = ref request.GetComponent<SpawnThreatRequest>();
                
                var threatEntity = World.CreateEntity();
                ref var cThreat = ref threatEntity.AddComponent<ThreatEventComponent>();
                
                cThreat.ThreatLevel = cSpawnRequest.ThreatLevel;
                cThreat.ExistTime = 0f;
                cThreat.Duration = cSpawnRequest.ThreadDuration;
                cThreat.TargetVertex = cSpawnRequest.TargetVertex;
                cThreat.DecayType = cSpawnRequest.DecayType;
            }
        }
    }
}