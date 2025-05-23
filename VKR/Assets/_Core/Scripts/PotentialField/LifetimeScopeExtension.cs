using VContainer;

namespace Game.PotentialField
{
    public static class LifetimeScopeExtension
    {
        public static IContainerBuilder RegisterPotentialField(this IContainerBuilder builder, AgentConfig agentConfig)
        {
            builder.RegisterInstance(new MapService()).AsSelf();
            builder.RegisterInstance(agentConfig).AsSelf();
            return builder;
        }
    }
}