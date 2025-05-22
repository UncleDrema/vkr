using Game.MapGraph;
using Game.Planning;
using Game.PotentialField;
using Game.SceneManagement;
using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.GameStartup
{
    internal class ApplicationLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private SceneRepository _sceneRepository;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterMapGraph();
            builder.RegisterPatrol();
            builder.RegisterPotentialField();
            builder.RegisterSceneManagement(_sceneRepository);
        }
    }
}