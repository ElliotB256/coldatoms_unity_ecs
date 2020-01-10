using ECSUtil;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// A system that calculates collisions between pairs of atoms.
/// 
/// Uses a hard-sphere model, which collides two atoms if they overlap and are within the same spatial region.
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
public class HardSphereCollisionSystem : JobComponentSystem
{
    /// <summary>
    /// Size of a collision cell, in Unity units.
    /// </summary>
    public static float COLLISION_CELL_SIZE = 1f;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        int atomNumber = AtomQuery.CalculateEntityCount();
        InitialiseMemory(atomNumber);

        // Collect atom data
        var masJob = new GetNativeArrayJob<Mass> { Array = Masses }.Schedule(AtomQuery, inputDeps);
        var posJob = new GetNativeArrayJob<Translation> { Array = Translations }.Schedule(AtomQuery, inputDeps);
        var radJob = new GetNativeArrayJob<CollisionRadius> { Array = Radii }.Schedule(AtomQuery, inputDeps);
        var velJob = new GetNativeArrayJob<Velocity> { Array = Velocities }.Schedule(AtomQuery, inputDeps);
        var getAtomData = JobHandle.CombineDependencies(JobHandle.CombineDependencies(masJob, posJob, radJob), velJob);

        var sortAtoms = new SortAtomsJob {
            BinnedAtoms = BinnedAtoms.AsParallelWriter(),
            CellSize = COLLISION_CELL_SIZE
        }.Schedule(AtomQuery, getAtomData);

        var getUniqueKeys = new GetFilledBoxIDs {
            BinnedAtoms = BinnedAtoms,
            UniqueKeys = UniqueBinIds
        }.Schedule(sortAtoms);

        var collisions = new DoCollisionsJob {
            BinIDs = UniqueBinIds,
            BinnedAtoms = BinnedAtoms,
            Translations = Translations,
            Masses = Masses,
            Radii = Radii,
            Velocities = Velocities,
            Collided = Collided
        }.Schedule(atomNumber, 1, getUniqueKeys);

        var updateAtomVelocities = new UpdateAtomVelocitiesJob { AtomVelocities = Velocities }.Schedule(AtomQuery, collisions);
        var updateCollisionStats = new UpdateCollisionStatsJob { Collided = Collided }.Schedule(AtomQuery, collisions);
        var updates = JobHandle.CombineDependencies(updateAtomVelocities, updateCollisionStats);

        // Dispose all quantities once the job is complete.
        Translations.Dispose(updates);
        Masses.Dispose(updates);
        Radii.Dispose(updates);
        Velocities.Dispose(updates);
        BinnedAtoms.Dispose(updates);
        UniqueBinIds.Dispose(updates);
        Collided.Dispose(updates);

        return updates;
    }

    public void InitialiseMemory(int atomNumber)
    {
        Translations = new NativeArray<Translation>(atomNumber, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        Masses = new NativeArray<Mass>(atomNumber, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        Radii = new NativeArray<CollisionRadius>(atomNumber, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        Velocities = new NativeArray<Velocity>(atomNumber, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        BinnedAtoms = new NativeMultiHashMap<int, int>(atomNumber, Allocator.TempJob);
        UniqueBinIds = new NativeList<int>(atomNumber, Allocator.TempJob);
        Collided = new NativeArray<bool>(atomNumber, Allocator.TempJob, NativeArrayOptions.ClearMemory);
    }

    protected override void OnCreateManager()
    {
        AtomQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<CollisionRadius>(),
                    ComponentType.ReadOnly<Mass>(),
                    ComponentType.ReadWrite<Velocity>(),
                    ComponentType.ReadWrite<Trapped>(),
                    ComponentType.ReadWrite<CollisionStats>()
                }
        });
    }

    EntityQuery AtomQuery;
    NativeArray<Translation> Translations;
    NativeArray<Mass> Masses;
    NativeArray<CollisionRadius> Radii;
    NativeArray<Velocity> Velocities;
    NativeList<int> UniqueBinIds;
    NativeArray<bool> Collided;

    /// <summary>
    /// Spatial Hashmap of Atoms. Key: bin, Values: atomId
    /// </summary>
    NativeMultiHashMap<int, int> BinnedAtoms;

    /// <summary>
    /// Gets atom quantities used for the collisions.
    /// </summary>
    [BurstCompile]
    struct GetAtomsJob : IJobForEachWithEntity<Translation, CollisionRadius, Mass, Velocity>
    {
        public NativeArray<Translation> Translations;
        public NativeArray<Mass> Masses;
        public NativeArray<CollisionRadius> Radii;
        public NativeArray<Velocity> Velocities;

        public void Execute(Entity entity, int index,
            [ReadOnly] ref Translation translation,
            [ReadOnly] ref CollisionRadius radius,
            [ReadOnly] ref Mass mass,
            [ReadOnly] ref Velocity velocity)
        {
            Translations[index] = translation;
            Radii[index] = radius;
            Masses[index] = mass;
            Velocities[index] = velocity;
        }
    }

    /// <summary>
    /// Sorts atoms into spatial hashmap
    /// </summary>
    [BurstCompile]
    struct SortAtomsJob : IJobForEachWithEntity<Translation>
    {
        [ReadOnly] public float CellSize;
        public NativeMultiHashMap<int, int>.ParallelWriter BinnedAtoms;

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation position)
        {
            int hash = (int)math.hash(new int3(math.floor(position.Value / CellSize)));
            BinnedAtoms.Add(hash, index);
        }
    }

    /// <summary>
    /// Gets the ids of hashmap boxes that contain atoms.
    /// </summary>
    struct GetFilledBoxIDs : IJob
    {
        [ReadOnly] public NativeMultiHashMap<int, int> BinnedAtoms;
        public NativeList<int> UniqueKeys;

        public void Execute()
        {
            var (keys, length) = BinnedAtoms.GetUniqueKeyArray(Allocator.Temp);
            for (int i = 0; i < length; i++)
                UniqueKeys.Add(keys[i]);
        }
    }

    [BurstCompile]
    struct DoCollisionsJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<int> BinIDs;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeMultiHashMap<int, int> BinnedAtoms;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<Translation> Translations;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<Mass> Masses;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<CollisionRadius> Radii;
        [NativeDisableParallelForRestriction] public NativeArray<Velocity> Velocities;
        [NativeDisableParallelForRestriction] public NativeArray<bool> Collided;

        /// <summary>
        /// Enumerate through each bin of the hash map, and collide atoms within that bin.
        /// </summary>
        /// <param name="index"></param>
        public void Execute(int index)
        {
            if (index >= BinIDs.Length)
                return;
            int binId = BinIDs[index];
            int outerLoopIndex = 0;
            BinnedAtoms.TryGetFirstValue(binId, out int outerID, out var outerIterator);
            do
            {
                int innerLoopIndex = 0;
                BinnedAtoms.TryGetFirstValue(binId, out int innerID, out var innerIterator);
                do
                {
                    if (Overlap(ref outerID, ref innerID))
                        TryCollide(outerID, innerID);
                    innerLoopIndex++;
                } while (innerLoopIndex < outerLoopIndex &&
                BinnedAtoms.TryGetNextValue(out innerID, ref innerIterator));
                outerLoopIndex++;
            } while (BinnedAtoms.TryGetNextValue(out outerID, ref outerIterator));
        }

        /// <summary>
        /// Returns true if the two ids overlap.
        /// </summary>
        public bool Overlap(ref int a, ref int b)
        {
            float distance2 = math.lengthsq(Translations[a].Value - Translations[b].Value);
            float size2 = math.pow(Radii[a].Value + Radii[b].Value, 2f);
            return distance2 < size2;
        }

        /// <summary>
        /// Ellastically collides two atoms.
        /// </summary>
        /// <param name="a">index of first atom</param>
        /// <param name="b">index of second atom</param>
        public void TryCollide(int a, int b)
        {
            // Velocity in center of mass frame
            float3 comv = (Velocities[a].Value + Velocities[b].Value) / 2f;

            //transform velocities to CoM frame
            float3 vel1 = Velocities[a].Value - comv;
            float3 vel2 = Velocities[b].Value - comv;

            //Only swap if velocities are facing each other in com frame
            if (math.dot(vel1, vel2) < 0f)
            {
                // swap velocities in CoM frame
                Velocities[a] = new Velocity { Value = comv + vel2 };
                Velocities[b] = new Velocity { Value = comv + vel1 };

                Collided[a] = true;
                Collided[b] = true;
            }
        }
    }

    [BurstCompile]
    struct UpdateAtomVelocitiesJob : IJobForEachWithEntity<Velocity>
    {
        public NativeArray<Velocity> AtomVelocities;

        public void Execute(Entity entity, int index, ref Velocity velocity)
        {
            velocity = AtomVelocities[index];
        }
    }

    [BurstCompile]
    struct UpdateCollisionStatsJob : IJobForEachWithEntity<CollisionStats>
    {
        public NativeArray<bool> Collided;

        public void Execute(Entity entity, int index, ref CollisionStats stats)
        {
            if (Collided[index])
                stats.TimeSinceLastCollision = 0f;
        }
    }
}