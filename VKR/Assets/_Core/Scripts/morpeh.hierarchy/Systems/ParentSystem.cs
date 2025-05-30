using Scellecs.Morpeh.Native;
using System;
using System.Diagnostics;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Hierarchy.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace Scellecs.Morpeh.Hierarchy
{
    public sealed class ParentSystem : LateUpdateSystem
    {
        private Filter deletedParentsFilter;
        private Filter newParentsFilter;
        private Filter removedParentsFilter;
        private Filter existingParentsFilter;

        private Stash<ChildComponent> childStash;
        private Stash<ParentComponent> parentStash;
        private Stash<PreviousParentComponent> previousParentStash;
        private Stash<ParentChangedRequest> parentChangedStash;

        public override void OnAwake()
        {
            // deletedParentsFilter = World.Filter
            //     .With<Child>()
            //     .Without<LocalToWorld>()
            //     .Build();

            newParentsFilter = World.Filter
                .With<ParentComponent>()
                .Without<PreviousParentComponent>()
                .Build();

            removedParentsFilter = World.Filter
                .With<PreviousParentComponent>()
                .Without<ParentComponent>()
                .Build();

            existingParentsFilter = World.Filter
                .With<ParentComponent>()
                .With<PreviousParentComponent>()
                .With<ParentChangedRequest>()
                .Build();

            childStash = World.GetStash<ChildComponent>();
            parentStash = World.GetStash<ParentComponent>();
            previousParentStash = World.GetStash<PreviousParentComponent>();
            parentChangedStash = World.GetStash<ParentChangedRequest>();
        }

        public override void OnUpdate(float deltaTime)
        {
            UpdateDeletedParents();
            UpdateRemoveParents();
            UpdateNewParents();
            UpdateChangeParents();
            CleanupChangedParentMarkers();
        }

        private void UpdateDeletedParents()
        {
            return;
            if (deletedParentsFilter.IsEmpty())
            {
                return;
            }

            var childEntities = new NativeQueue<Entity>(Allocator.TempJob);
            var parentsFilter = deletedParentsFilter.AsNative();

            new GatherChildEntitiesJob()
            {
                parentsFilter = parentsFilter,
                childStash = childStash.AsNative(),
                parentsStash = parentStash.AsNative(),
                children = childEntities.AsParallelWriter()
            }
            .ScheduleParallel(parentsFilter.length, 32, default).Complete();

            while (childEntities.TryDequeue(out var childEnt))
            {
                parentStash.Remove(childEnt);
                previousParentStash.Remove(childEnt);
            }

            childEntities.Dispose(default).Complete();

            foreach (var entity in deletedParentsFilter)
            {
                childStash.Remove(entity);
            }

            World.Commit();
        }

        private void UpdateRemoveParents()
        {
            foreach (var childEntity in removedParentsFilter)
            {
                ref var prevParent = ref previousParentStash.Get(childEntity);

                RemoveChildFromParent(childEntity, prevParent.Value);
                previousParentStash.Remove(childEntity);
            }

            World.Commit();

            void RemoveChildFromParent(Entity childEntity, Entity parentEntity)
            {
                ref var children = ref childStash.Get(parentEntity, out bool hasChildren).Value;

                if (hasChildren)
                {
                    var childIndex = FindChildIndex(children, childEntity);
                    children.RemoveAt(childIndex);

                    if (children.Length == 0)
                    {
                        childStash.Remove(parentEntity);
                    }
                }
            }

            int FindChildIndex(NativeList<Entity> children, Entity entity)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i] == entity)
                        return i;
                }

                throw new InvalidOperationException("Child entity not in parent");
            }
        }

        private void UpdateNewParents()
        {
            foreach (var entity in newParentsFilter)
            {
                previousParentStash.Set(entity, new PreviousParentComponent() { Value = default });
            }

            World.Commit();
        }

        private unsafe void UpdateChangeParents()
        {
            if (existingParentsFilter.IsEmpty())
            {
                return;
            }

            var parentsFilterNative = existingParentsFilter.AsNative();
            var count = parentsFilterNative.length * 2;

            var parentChildrenToRemove = new NativeParallelMultiHashMap<Entity, Entity>(count, Allocator.TempJob);
            var parentChildrenToAdd = new NativeParallelMultiHashMap<Entity, Entity>(count, Allocator.TempJob);
            var uniqueParents = new NativeParallelHashMap<Entity, int>(count, Allocator.TempJob);
            var childParentToRemove = new NativeParallelHashSet<Entity>(count, Allocator.TempJob);

            var gatherChangedParentsJobHandle = new GatherChangedParentsJob
            {
                parentChildrenToAdd = parentChildrenToAdd.AsParallelWriter(),
                parentChildrenToRemove = parentChildrenToRemove.AsParallelWriter(),
                childParentToRemove = childParentToRemove.AsParallelWriter(),
                uniqueParents = uniqueParents.AsParallelWriter(),
                world = World.AsNative(),
                existingParentsFilter = existingParentsFilter.AsNative(),
                parentStash = parentStash.AsNative(),
                previousParentStash = previousParentStash.AsNative(),
                childStash = childStash.AsNative(),
            }
            .ScheduleParallel(parentsFilterNative.length, 16, default);
            gatherChangedParentsJobHandle.Complete();

            foreach (var entity in childParentToRemove)
            {
                parentStash.Remove(entity);
            }

            World.Commit();

            var parentsMissingChild = new NativeList<Entity>(Allocator.TempJob);

            var parentsMissingChildHandle = new FindMissingChildJob()
            {
                childStash = childStash.AsNative(),
                uniqueParents = uniqueParents,
                parentsMissingChild = parentsMissingChild
            }
            .Schedule();
            parentsMissingChildHandle.Complete();

            for (int i = 0; i < parentsMissingChild.Length; i++)
            {
                childStash.Set(parentsMissingChild[i], new ChildComponent() { Value = new NativeList<Entity>(Allocator.Persistent) });
            }

            World.Commit();

            new FixupChangedChildrenJob()
            {
                childStash = childStash.AsNative(),
                parentChildrenToAdd = parentChildrenToAdd,
                parentChildrenToRemove = parentChildrenToRemove,
                uniqueParents = uniqueParents
            }
            .Schedule().Complete();

            var parents = uniqueParents.GetKeyArray(Allocator.Temp);

            foreach (var parentEntity in parents)
            {
                var children = childStash.Get(parentEntity);

                if (children.Value.Length == 0)
                {
                    childStash.Remove(parentEntity);
                }
            }

            JobHandle* disposeHandles = stackalloc JobHandle[5];
            disposeHandles[0] = parentChildrenToRemove.Dispose(default);
            disposeHandles[1] = parentChildrenToAdd.Dispose(default);
            disposeHandles[2] = uniqueParents.Dispose(default);
            disposeHandles[3] = childParentToRemove.Dispose(default);
            disposeHandles[4] = parentsMissingChild.Dispose(default);

            JobHandleUnsafeUtility.CombineDependencies(disposeHandles, 5).Complete();
        }

        private void CleanupChangedParentMarkers() => parentChangedStash.RemoveAll();

        public void Dispose() { }
    }

    [BurstCompile]
    internal struct GatherChildEntitiesJob : IJobFor
    {
        [ReadOnly] public NativeFilter parentsFilter;
        [ReadOnly] public NativeStash<ChildComponent> childStash;
        [ReadOnly] public NativeStash<ParentComponent> parentsStash;
        [WriteOnly] public NativeQueue<Entity>.ParallelWriter children;

        public void Execute(int index)
        {
            var parentEntityId = parentsFilter[index];
            var child = childStash.Get(parentEntityId);

            for (int i = 0; i < child.Value.Length; i++)
            {
                var childEntityId = child.Value[i];
                var parentFromChild = parentsStash.Get(childEntityId, out bool hasParent);

                if (hasParent && parentFromChild.Value == parentEntityId)
                {
                    children.Enqueue(childEntityId);
                }
            }
        }
    }

    [BurstCompile]
    internal struct FixupChangedChildrenJob : IJob
    {
        public NativeStash<ChildComponent> childStash;

        [ReadOnly] public NativeParallelMultiHashMap<Entity, Entity> parentChildrenToAdd;
        [ReadOnly] public NativeParallelMultiHashMap<Entity, Entity> parentChildrenToRemove;
        [ReadOnly] public NativeParallelHashMap<Entity, int> uniqueParents;

        public void Execute()
        {
            var parents = uniqueParents.GetKeyArray(Allocator.Temp);

            for (int i = 0; i < parents.Length; i++)
            {
                var parent = parents[i];
                ref var children = ref childStash.Get(parent, out bool hasChildren);

                if (hasChildren)
                {
                    RemoveChildrenFromParent(parent, ref children.Value);
                    AddChildrenToParent(parent, ref children.Value);
                }
            }
        }

        void AddChildrenToParent(Entity parent, ref NativeList<Entity> children)
        {
            if (parentChildrenToAdd.TryGetFirstValue(parent, out var child, out var it))
            {
                do
                {
                    children.Add(child);
                }
                while (parentChildrenToAdd.TryGetNextValue(out child, ref it));
            }
        }

        void RemoveChildrenFromParent(Entity parent, ref NativeList<Entity> children)
        {
            if (parentChildrenToRemove.TryGetFirstValue(parent, out var child, out var it))
            {
                do
                {
                    var childIndex = FindChildIndex(ref children, child);
                    children.RemoveAt(childIndex);
                }
                while (parentChildrenToRemove.TryGetNextValue(out child, ref it));
            }
        }

        int FindChildIndex(ref NativeList<Entity> children, Entity entity)
        {
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] == entity)
                    return i;
            }

            ThrowChildEntityNotInParent();
            return -1;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void ThrowChildEntityNotInParent()
        {
            throw new InvalidOperationException("Child entity not in parent");
        }
    }

    [BurstCompile]
    internal struct FindMissingChildJob : IJob
    {
        public NativeStash<ChildComponent> childStash;
        public NativeParallelHashMap<Entity, int> uniqueParents;
        public NativeList<Entity> parentsMissingChild;

        public void Execute()
        {
            var parents = uniqueParents.GetKeyArray(Allocator.Temp);

            for (int i = 0; i < parents.Length; i++)
            {
                var parent = parents[i];

                if (childStash.Has(parent) == false)
                {
                    parentsMissingChild.Add(parent);
                }
            }
        }
    }

    [BurstCompile]
    internal struct GatherChangedParentsJob : IJobFor
    {
        public NativeWorld world;
        public NativeFilter existingParentsFilter;
        public NativeStash<ParentComponent> parentStash;
        public NativeStash<PreviousParentComponent> previousParentStash;
        public NativeStash<ChildComponent> childStash;

        public NativeParallelMultiHashMap<Entity, Entity>.ParallelWriter parentChildrenToAdd;
        public NativeParallelMultiHashMap<Entity, Entity>.ParallelWriter parentChildrenToRemove;
        public NativeParallelHashSet<Entity>.ParallelWriter childParentToRemove;
        public NativeParallelHashMap<Entity, int>.ParallelWriter uniqueParents;

        public void Execute(int index)
        {
            var childEntityId = existingParentsFilter[index];

            ref var parent = ref parentStash.Get(childEntityId);
            ref var previousParent = ref previousParentStash.Get(childEntityId);

            if (parent.Value != previousParent.Value)
            {
                if (world.Has(parent.Value) == false)
                {
                    childParentToRemove.Add(childEntityId);
                    return;
                }

                parentChildrenToAdd.Add(parent.Value, childEntityId);
                uniqueParents.TryAdd(parent.Value, 0);

                if (childStash.Has(previousParent.Value))
                {
                    parentChildrenToRemove.Add(previousParent.Value, childEntityId);
                    uniqueParents.TryAdd(previousParent.Value, 0);
                }

                previousParent.Value = parent.Value;
            }
        }
    }
}