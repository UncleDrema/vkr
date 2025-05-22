using VContainer;

namespace Game.PotentialField
{
    public static class LifetimeScopeExtension
    {
        public static IContainerBuilder RegisterPotentialField(this IContainerBuilder builder)
        {
            builder.RegisterInstance(new MapService()).AsSelf();
            return builder;
        }
    }
}