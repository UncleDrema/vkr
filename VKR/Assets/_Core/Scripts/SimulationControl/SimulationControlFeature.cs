using Game.SceneManagement.Api;
using Game.SimulationControl.Requests;
using Game.SimulationControl.Systems;
using Scellecs.Morpeh.Addons.Feature;

namespace Game.SimulationControl
{
    public class SimulationControlFeature : UpdateFeature
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly SimulationService _simulationService;
        
        public SimulationControlFeature(ISceneLoader sceneLoader, SimulationService simulationService)
        {
            _sceneLoader = sceneLoader;
            _simulationService = simulationService;
        }
        
        protected override void Initialize()
        {
            RegisterRequest<InitializeAfterSceneLoadedRequest>();
            RegisterRequest<StartSimulationRequest>();
            RegisterRequest<CollectStatisticsRequest>();
            
            AddSystem(new StartSimulationSystem(_sceneLoader, _simulationService));
            AddSystem(new UpdateSimulationSystem());
        }
    }
}