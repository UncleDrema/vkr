using Game.MapGraph.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Feature.Events;
using TriInspector;
using UnityEngine;

namespace Game.MapGraph
{
    public class GraphHelper : MonoBehaviour
    {
        [Button]
        public void SpawnThreatByPosition(float level, float duration, ThreatDecayType decayType)
        {
            ref var cSpawnReq = ref World.Default.CreateEventEntity<SpawnThreatByPositionRequest>();
            cSpawnReq.Position = transform.position;
            cSpawnReq.ThreatLevel = level;
            cSpawnReq.ThreadDuration = duration;
            cSpawnReq.DecayType = decayType;
        }

        [Button]
        public void InitializeGraphZones(int zoneCount)
        {
            ref var cInitReq = ref World.Default.CreateEventEntity<InitializeGraphZonesRequest>();
            cInitReq.ZoneCount = zoneCount;
        }
    }
}