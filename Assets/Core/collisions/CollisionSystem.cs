using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// A system that calculates collisions between pairs of atoms.
/// 
/// For fast performance, a HashMap Monte-Carlo method is used.
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
public class CollisionSystem : JobComponentSystem
{
    public static float COLLISION_CELL_SIZE = 1f;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Cleanup();

        int atomNumber = AtomQuery.CalculateEntityCount();
        Atoms = new NativeArray<Atom>(atomNumber, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        AtomVelocities = new NativeArray<Velocity>(atomNumber, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        BinnedAtoms = new NativeMultiHashMap<int, int>(atomNumber, Allocator.TempJob);
        UniqueBinIds = new NativeList<int>(atomNumber, Allocator.TempJob);
        Collided = new NativeArray<bool>(atomNumber, Allocator.TempJob, NativeArrayOptions.ClearMemory);

        var clearCollideJH = new ClearCollideJob { Collided = Collided }.Schedule(inputDeps);
        var getAtomDataJH = new GetAtomDataJob { Atoms = Atoms, AtomVelocities = AtomVelocities }.Schedule(AtomQuery, clearCollideJH);
        var sortAtomsJH = new SortAtomsJob { BinnedAtoms = BinnedAtoms.AsParallelWriter(), CellSize = COLLISION_CELL_SIZE }.Schedule(AtomQuery, getAtomDataJH);
        var getUniqueKeysJH = new GetUniqueKeysJob { BinnedAtoms = BinnedAtoms, UniqueKeys = UniqueBinIds }.Schedule(sortAtomsJH);
        var doCollisionsJH = new DoCollisionsJob { Atoms = Atoms, AtomVelocities = AtomVelocities, BinIDs = UniqueBinIds, BinnedAtoms = BinnedAtoms, Collided = Collided }.Schedule(atomNumber, 1, getUniqueKeysJH);
        var updateAtomVelocitiesJH = new UpdateAtomVelocitiesJob { AtomVelocities = AtomVelocities }.Schedule(AtomQuery, doCollisionsJH);
        var updateCollisionStatsJH = new UpdateCollisionStatsJob { Collided = Collided }.Schedule(AtomQuery, doCollisionsJH);

        _hasRun = true;
        return JobHandle.CombineDependencies(updateAtomVelocitiesJH, updateCollisionStatsJH);
    }

    protected override void OnCreateManager()
    {
        //Enabled = false;
        AtomQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<ScatteringRadius>(),
                    ComponentType.ReadOnly<Mass>(),
                    ComponentType.ReadWrite<Velocity>(),
                    ComponentType.ReadWrite<Trapped>(),
                    ComponentType.ReadWrite<CollisionStats>()
                }
        });
    }

    EntityQuery AtomQuery;

    struct Atom
    {
        public Translation translation;
        public ScatteringRadius radius;
        public Mass mass;
    }

    NativeArray<Atom> Atoms;
    NativeArray<Velocity> AtomVelocities;
    NativeList<int> UniqueBinIds;
    NativeArray<bool> Collided;

    bool _hasRun = false;
    public void Cleanup()
    {
        if (_hasRun)
        {
            Atoms.Dispose();
            AtomVelocities.Dispose();
            UniqueBinIds.Dispose();
            BinnedAtoms.Dispose();
            Collided.Dispose();
        }
    }

    protected override void OnDestroyManager()
    {
        Cleanup();
    }

    /// <summary>
    /// Spatial Hashmap of Atoms. Key: bin, Values: atomId
    /// </summary>
    NativeMultiHashMap<int, int> BinnedAtoms;

    [BurstCompile]
    struct GetAtomDataJob : IJobForEachWithEntity<Translation, ScatteringRadius, Mass, Velocity>
    {
        public NativeArray<Atom> Atoms;
        public NativeArray<Velocity> AtomVelocities;

        public void Execute(Entity entity, int index,
            [ReadOnly] ref Translation translation,
            [ReadOnly] ref ScatteringRadius radius,
            [ReadOnly] ref Mass mass,
            [ReadOnly] ref Velocity velocity)
        {
            Atoms[index] = new Atom { translation = translation, radius = radius, mass = mass };
            AtomVelocities[index] = velocity;
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

    struct GetUniqueKeysJob : IJob
    {
        [ReadOnly] public NativeMultiHashMap<int, int> BinnedAtoms;
        public NativeList<int> UniqueKeys;

        public void Execute()
        {
            var (keys, length) = BinnedAtoms.GetUniqueKeyArray(Allocator.Temp);
            //keys.CopyTo(UniqueKeys);
            //    UniqueKeys.AddRange(NativeArrayUnsafeUtility.GetUnsafePtr(keys), length);
            //UniqueKeys.Capacity = length;
            for (int i = 0; i < length; i++)
                UniqueKeys.Add(keys[i]);
        }
    }

    [BurstCompile]
    struct DoCollisionsJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<Atom> Atoms;
        [NativeDisableParallelForRestriction] public NativeArray<Velocity> AtomVelocities;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<int> BinIDs;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeMultiHashMap<int, int> BinnedAtoms;
        /// <summary>
        /// Set to true for atoms that have collided
        /// </summary>
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
            int innerLoopIndex = 0;
            BinnedAtoms.TryGetFirstValue(binId, out int outerID, out var outerIterator);
            do
            {
                innerLoopIndex = 0;
                BinnedAtoms.TryGetFirstValue(binId, out int innerID, out var innerIterator);
                do
                {
                    if (TestCollision(ref outerID, ref innerID))
                    {
                        Collide(ref outerID, ref innerID);
                    }
                    innerLoopIndex++;
                } while (innerLoopIndex < outerLoopIndex &&
                BinnedAtoms.TryGetNextValue(out innerID, ref innerIterator));
                outerLoopIndex++;
            } while (BinnedAtoms.TryGetNextValue(out outerID, ref outerIterator));
        }

        public bool TestCollision(ref int id1, ref int id2)
        {
            bool overlaps = math.lengthsq(Atoms[id1].translation.Value - Atoms[id2].translation.Value) < math.pow(Atoms[id1].radius.Value + Atoms[id2].radius.Value, 2f);
            return overlaps;
        }

        public void Collide(ref int id1, ref int id2)
        {
            // Velocity in center of mass frame
            float3 comv = (AtomVelocities[id1].Value + AtomVelocities[id2].Value) / 2f;

            //transform velocities to CoM frame
            float3 vel1 = AtomVelocities[id1].Value - comv;
            float3 vel2 = AtomVelocities[id2].Value - comv;

            //Only swap if velocities are facing each other in com frame
            if (math.dot(vel1, vel2) < 0f)
            {
                // swap velocities in CoM frame
                AtomVelocities[id1] = new Velocity { Value = comv + vel2 };
                AtomVelocities[id2] = new Velocity { Value = comv + vel1 };

                Collided[id1] = true;
                Collided[id2] = true;
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

    struct ClearCollideJob : IJob
    {
        public NativeArray<bool> Collided;

        public void Execute()
        {
            for (int i = 0; i < Collided.Length; i++)
                Collided[i] = false;
        }
    }
}