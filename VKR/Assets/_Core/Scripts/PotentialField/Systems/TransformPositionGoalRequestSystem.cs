using Game.PotentialField.Requests;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;

namespace Game.PotentialField.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TransformPositionGoalRequestSystem : UpdateSystem
    {
        private readonly MapService _mapService;
        
        private Filter _positionGoalRequests;
        
        public TransformPositionGoalRequestSystem(MapService mapService)
        {
            _mapService = mapService;
        }
        
        public override void OnAwake()
        {
            _positionGoalRequests = World.Filter
                .With<SetGoalByPositionSelfRequest>()
                .Build();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var req in _positionGoalRequests)
            {
                ref var cSetGoalByPositionRequest = ref req.GetComponent<SetGoalByPositionSelfRequest>();
                
                var mapPosition = _mapService.WorldToMapPosition(cSetGoalByPositionRequest.Position);
                
                ref var cSetGoalRequest = ref req.AddComponent<SetFieldGoalSelfRequest>();
                cSetGoalRequest.X = mapPosition.x;
                cSetGoalRequest.Y = mapPosition.y;
                cSetGoalRequest.RetryCount = 10;
            }
        }
    }
}