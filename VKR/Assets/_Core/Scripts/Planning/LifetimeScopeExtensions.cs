using VContainer;

namespace Game.Planning
{
    public static class LifetimeScopeExtensions
    {
        public static IContainerBuilder RegisterPatrol(this IContainerBuilder builder)
        {
            builder.RegisterInstance(new PatrolService()).AsSelf();
            return builder;
        }
    }
}