using Game.PotentialField.Events;
using Game.PotentialField.Requests;
using Game.PotentialField.Systems;
using Scellecs.Morpeh.Addons.Feature;

namespace Game.PotentialField
{
    public class PotentialFieldFeature : UpdateFeature
    {
        protected override void Initialize()
        {
            RegisterRequest<InitializeMapSelfRequest>();
            RegisterRequest<InitializeAgentSelfRequest>();
            RegisterRequest<SetFieldGoalSelfRequest>();
            RegisterRequest<ClearFieldGoalRequest>();
            
            AddSystem(new InitializeMapSystem());
            AddSystem(new InitializeAgentSystem());
            AddSystem(new UpdateAgentPositionSystem());
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