using Game.MapGraph;
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
            builder.RegisterSceneManagement(_sceneRepository);
        }
    }
}