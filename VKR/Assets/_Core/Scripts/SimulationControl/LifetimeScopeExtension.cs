using VContainer;

namespace Game.SimulationControl
{
    public static class LifetimeScopeExtension
    {
        public static IContainerBuilder RegisterSimulation(this IContainerBuilder builder)
        {
            builder.RegisterInstance(new SimulationService()).AsSelf();

            return builder;
        }
    }
}