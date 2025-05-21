using Game.SceneManagement.Api;
using Game.SceneManagement.Core;
using VContainer;

namespace Game.SceneManagement
{
    public static class LifetimeScopeExtension
    {
        public static IContainerBuilder RegisterSceneManagement(this IContainerBuilder builder,
            SceneRepository sceneRepository)
        {
            builder.RegisterInstance(sceneRepository);
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Scoped);

            return builder;
        }
    }
}