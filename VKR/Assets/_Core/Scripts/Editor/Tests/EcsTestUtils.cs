using Scellecs.Morpeh;

namespace Game.Editor.Tests
{
    public static class EcsTestUtils
    {
        internal static float DeltaTime60Fps = 1f / 60;
        
        public static World CreateWorld(params ISystem[] systems)
        {
            World world = World.Create();

            if (systems.Length > 0)
            {
                SystemsGroup systemsGroup = world.CreateSystemsGroup();
                foreach (var system in systems)
                    systemsGroup.AddSystem(system);
                world.AddSystemsGroup(0, systemsGroup);
            }

            return world;
        }

        public static World CreateWorld(IInitializer[] initializers, ISystem[] systems)
        {
            World world = World.Create();

            if ( initializers.Length > 0 || systems.Length > 0)
            {
                SystemsGroup systemsGroup = world.CreateSystemsGroup();

                foreach (var initializer in initializers)
                    systemsGroup.AddInitializer(initializer);

                foreach (var system in systems)
                    systemsGroup.AddSystem(system);

                world.AddSystemsGroup(0, systemsGroup);
            }

            return world;
        }

        internal static void Update60Fps(this World world)
        {
            world.Update(DeltaTime60Fps);
        }

        internal static void Update60Fps(this World world, float duration)
        {
            float pasedTime = 0f;
            while (pasedTime < duration)
            {
                world.Update(DeltaTime60Fps);
                pasedTime += DeltaTime60Fps;
            }
        }

        internal static Filter GetFilter<T>(this World world)
            where T : struct, IComponent
        {
            return world.Filter.With<T>().Build();
        }
        
        public delegate void ActionComponent<T>(ref T component)
            where T : struct, IComponent;
        
        internal static void ForEach<T>(this World world, ActionComponent<T> action)
        where T : struct, IComponent
        {
            foreach (var e in world.GetFilter<T>())
            {
                ref var component = ref e.GetComponent<T>();
                action(ref component);
            }
        }
        
        public delegate void EntityActionComponent<T>(Entity e, ref T component)
            where T : struct, IComponent;
        
        internal static void ForEach<T>(this World world, EntityActionComponent<T> action)
            where T : struct, IComponent
        {
            foreach (var e in world.GetFilter<T>())
            {
                ref var component = ref e.GetComponent<T>();
                action(e, ref component);
            }
        }

        internal static int CountOf<T>(this World world)
        where T : struct, IComponent
        {
            return world.GetFilter<T>().GetLengthSlow();
        }

        public delegate bool PredicateComponent<T>(ref T component)
            where T : struct, IComponent;

        internal static bool Any<T>(this World world, PredicateComponent<T> predicate)
            where T : struct, IComponent
        {
            foreach (var e in world.GetFilter<T>())
            {
                ref var component = ref e.GetComponent<T>();
                if (predicate(ref component))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool All<T>(this World world, PredicateComponent<T> predicate)
            where T : struct, IComponent
        {
            foreach (var e in world.GetFilter<T>())
            {
                ref var component = ref e.GetComponent<T>();
                if (!predicate(ref component))
                {
                    return false;
                }
            }

            return true;
        }

        public static void UpdateWorld(World world)
        {
            world.Update(DeltaTime60Fps);
        }
    }
}