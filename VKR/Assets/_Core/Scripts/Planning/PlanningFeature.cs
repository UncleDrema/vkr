using Game.MapGraph;
using Game.Planning.Systems;
using Scellecs.Morpeh.Addons.Feature;

namespace Game.Planning
{
    public class PlanningFeature : UpdateFeature
    {
        private readonly GraphService _graphService;
        private readonly PatrolService _patrolService;
        
        public PlanningFeature(GraphService graphService, PatrolService patrolService)
        {
            _graphService = graphService;
            _patrolService = patrolService;
        }
        
        protected override void Initialize()
        {
            AddSystem(new UpdateVisibleVerticesSystem(_graphService));
            AddSystem(new ClearAgentPatrolTargetOnReachedOrFailedSystem());
            AddSystem(new ZoneAssignmentSystem());
            AddSystem(new PatrolTargetSelectionSystem(_graphService, _patrolService));
        }
    }
}