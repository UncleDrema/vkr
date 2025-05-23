using Game.MapGraph;
using Game.PotentialField.Events;
using Game.PotentialField.Requests;
using Game.PotentialField.Systems;
using Game.SimulationControl;
using Scellecs.Morpeh.Addons.Feature;

namespace Game.PotentialField
{
    public class PotentialFieldFeature : UpdateFeature
    {
        private readonly SimulationService _simulationService;
        private readonly MapService _mapService;
        private readonly GraphService _graphService;
        private readonly AgentConfig _agentConfig;
        
        public PotentialFieldFeature(MapService mapService, SimulationService simulationService, GraphService graphService, AgentConfig agentConfig)
        {
            _mapService = mapService;
            _simulationService = simulationService;
            _graphService = graphService;
            _agentConfig = agentConfig;
        }
        
        protected override void Initialize()
        {
            RegisterRequest<InitializeMapSelfRequest>();
            RegisterRequest<InitializeAgentSelfRequest>();
            RegisterRequest<SetFieldGoalSelfRequest>();
            RegisterRequest<ClearFieldGoalRequest>();
            RegisterRequest<SetGoalByPositionSelfRequest>();
            RegisterRequest<SpawnAgentsRequest>();
         
            AddInitializer(new InitializeMapServiceSystem(_mapService));
            AddSystem(new SpawnAgentsSystem(_agentConfig));
            AddSystem(new InitializeMapSystem(_simulationService));
            AddSystem(new InitializeAgentSystem(_simulationService));
            AddSystem(new UpdateAgentPositionSystem(_mapService));
            AddSystem(new TransformPositionGoalRequestSystem(_mapService));
            AddSystem(new SetFieldGoalSystem());
            AddSystem(new UpdateAgentLocalMapSystem(_simulationService));
            AddSystem(new LocalFieldMovementSystem(_simulationService, _graphService));
            AddSystem(new ClearFieldGoalSystem());
            AddSystem(new SimpleMovementSystem(_simulationService));
            
            RegisterEvent<GoalReachedEvent>();
            RegisterEvent<SetGoalSuccessEvent>();
            RegisterEvent<SetGoalFailEvent>();
            RegisterEvent<AgentsCreatedEvent>();
        }
    }
}