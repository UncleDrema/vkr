using Game.PotentialField.Events;
using Game.PotentialField.Requests;
using Game.PotentialField.Systems;
using Scellecs.Morpeh.Addons.Feature;

namespace Game.PotentialField
{
    public class PotentialFieldFeature : UpdateFeature
    {
        private readonly MapService _mapService;
        
        public PotentialFieldFeature(MapService mapService)
        {
            _mapService = mapService;
        }
        
        protected override void Initialize()
        {
            RegisterRequest<InitializeMapSelfRequest>();
            RegisterRequest<InitializeAgentSelfRequest>();
            RegisterRequest<SetFieldGoalSelfRequest>();
            RegisterRequest<ClearFieldGoalRequest>();
            RegisterRequest<SetGoalByPositionSelfRequest>();
         
            AddInitializer(new InitializeMapServiceSystem(_mapService));
            AddSystem(new InitializeMapSystem());
            AddSystem(new InitializeAgentSystem());
            AddSystem(new UpdateAgentPositionSystem(_mapService));
            AddSystem(new TransformPositionGoalRequestSystem(_mapService));
            AddSystem(new SetFieldGoalSystem());
            AddSystem(new UpdateAgentLocalMapSystem());
            AddSystem(new LocalFieldMovementSystem());
            AddSystem(new ClearFieldGoalSystem());
            
            RegisterEvent<GoalReachedEvent>();
            RegisterEvent<SetGoalSuccessEvent>();
            RegisterEvent<SetGoalFailEvent>();
        }
    }
}