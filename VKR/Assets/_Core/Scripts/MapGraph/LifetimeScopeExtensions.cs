using VContainer;

namespace Game.MapGraph
{
    public static class LifetimeScopeExtensions
    {
        public static IContainerBuilder RegisterMapGraph(this IContainerBuilder builder)
        {
            builder.RegisterInstance(new GraphService()).AsSelf();
            return builder;
        }
    }
}