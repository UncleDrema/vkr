using Game.MapGraph.Events;
using Game.MapGraph.Requests;
using Game.MapGraph.Systems;
using Game.SimulationControl;
using Scellecs.Morpeh.Addons.Feature;

namespace Game.MapGraph
{
    public class MapGraphFeature : UpdateFeature
    {
        private readonly GraphService _graphService;
        private readonly SimulationService _simulationService;
        
        public MapGraphFeature(GraphService graphService, SimulationService simulationService)
        {
            _graphService = graphService;
            _simulationService = simulationService;
        }
        
        protected override void Initialize()
        {
            RegisterRequest<InitializeGraphFromUnityRequest>();
            RegisterRequest<SpawnThreatByPositionRequest>();
            RegisterRequest<SpawnThreatRequest>();
            RegisterRequest<InitializeGraphZonesRequest>();
            RegisterRequest<InitializeGraphFromUnityRequest>();
         
            AddInitializer(new InitializeGraphService(_graphService));
            AddSystem(new TransformUnityInitGraphRequestSystem());
            AddSystem(new InitializeGraphSystem());
            AddSystem(new InitializeGraphZonesSystem());
            AddSystem(new TransformSpawnThreatRequestSystem(_graphService));
            AddSystem(new SpawnThreatSystem());
            AddSystem(new UpdateThreatSystem());
            AddSystem(new ZoneDriftSystem(_simulationService));
            
            RegisterEvent<ZonesInitializedEvent>();
        }
    }
}