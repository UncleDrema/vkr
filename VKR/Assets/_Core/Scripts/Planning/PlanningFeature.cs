using Game.MapGraph;
using Game.Planning.Requests;
using Game.Planning.Systems;
using Game.SimulationControl;
using Scellecs.Morpeh.Addons.Feature;

namespace Game.Planning
{
    public class PlanningFeature : UpdateFeature
    {
        private readonly SimulationService _simulationService;
        private readonly GraphService _graphService;
        private readonly PatrolService _patrolService;
        
        public PlanningFeature(GraphService graphService, PatrolService patrolService, SimulationService simulationService)
        {
            _graphService = graphService;
            _patrolService = patrolService;
            _simulationService = simulationService;
        }
        
        protected override void Initialize()
        {
            RegisterRequest<SelectSimpleMovementGoalRequest>();
            
            AddSystem(new UpdateVisibleVerticesSystem(_graphService));
            AddSystem(new ClearAgentPatrolTargetOnReachedOrFailedSystem());
            AddSystem(new ZoneAssignmentSystem());
            AddSystem(new PatrolTargetSelectionSystem(_graphService, _patrolService, _simulationService));
            AddSystem(new SimpleMovementPlanningSystem(_simulationService));
            AddSystem(new SimpleMovementNextGoalSystem(_graphService));
        }
    }
}