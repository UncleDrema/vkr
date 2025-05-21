using Game.MapGraph.Requests;
using Game.MapGraph.Systems;
using Scellecs.Morpeh.Addons.Feature;

namespace Game.MapGraph
{
    public class MapGraphFeature : UpdateFeature
    {
        private readonly GraphService _graphService;
        
        public MapGraphFeature(GraphService graphService)
        {
            _graphService = graphService;
        }
        
        protected override void Initialize()
        {
            RegisterRequest<InitializeGraphRequest>();
            RegisterRequest<SpawnThreatByPositionRequest>();
            RegisterRequest<SpawnThreatRequest>();
         
            AddInitializer(new InitializeGraphService(_graphService));
            AddSystem(new InitializeGraphSystem());
            AddSystem(new TransformSpawnThreatRequestSystem(_graphService));
            AddSystem(new SpawnThreatSystem());
            AddSystem(new UpdateThreatSystem());
        }
    }
}