using Game.MapGraph;
using Game.Planning;
using Game.PotentialField;
using Game.SceneManagement;
using Game.SimulationControl;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.GameStartup
{
    internal class ApplicationLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private SceneRepository _sceneRepository;
        
        [SerializeField]
        private AgentConfig _agentConfig;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterMapGraph();
            builder.RegisterPatrol();
            builder.RegisterPotentialField(_agentConfig);
            builder.RegisterSimulation();
            builder.RegisterSceneManagement(_sceneRepository);
        }
    }
}