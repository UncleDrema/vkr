using Game.MapGraph.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;

namespace Game.MapGraph.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TransformUnityInitGraphRequestSystem : UpdateSystem
    {
        private Filter _unityInitGraphRequests;
        
        public override void OnAwake()
        {
            _unityInitGraphRequests = World.Filter
                .With<InitializeGraphFromUnityRequest>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var req in _unityInitGraphRequests)
            {
                ref var cInitGraphUnityRequest = ref req.GetComponent<InitializeGraphFromUnityRequest>();
                ref var cInitGraphRequest = ref req.AddComponent<InitializeGraphRequest>();
                cInitGraphRequest.DesiredSegmentLength = cInitGraphUnityRequest.DesiredSegmentLength;
                cInitGraphRequest.Nodes = cInitGraphUnityRequest.LevelGraphData.nodes;
                cInitGraphRequest.Edges = cInitGraphUnityRequest.LevelGraphData.edges;
            }
        }
    }
}