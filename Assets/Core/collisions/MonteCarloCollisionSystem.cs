using Integration;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Calculates collisions between pairs of atoms using a Monte-Carlo method.
/// 
/// Note currently implemented - disabled by default.
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
public class MonteCarloCollisionSystem : JobComponentSystem
{
    /// <summary>
    /// Size of a collision cell, in Unity units.
    /// </summary>
    public static float COLLISION_CELL_SIZE = 1f;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        int atomNumber = AtomQuery.CalculateEntityCount();
        InitialiseMemory(atomNumber);

        var clearCollideJH = new ClearCollideJob { Collided = Collided }.Schedule(inputDeps);
        var getAtomDataJH = new GetAtomDataJob { Atoms = Atoms, AtomVelocities = AtomVelocities }.Schedule(AtomQuery, clearCollideJH);
        var sortAtomsJH = new SortAtomsJob { BinnedAtoms = BinnedAtoms.AsParallelWriter(), CellSize = COLLISION_CELL_SIZE }.Schedule(AtomQuery, getAtomDataJH);
        var getUniqueKeysJH = new GetUniqueKeysJob { BinnedAtoms = BinnedAtoms, UniqueKeys = UniqueBinIds }.Schedule(sortAtomsJH);
        var doCollisionsJH = new DoCollisionsJob { dT = FixedUpdateGroup.FIXED_TIME_DELTA, Atoms = Atoms, AtomVelocities = AtomVelocities, BinIDs = UniqueBinIds, BinnedAtoms = BinnedAtoms, Collided = Collided }.Schedule(atomNumber, 1, getUniqueKeysJH);
        var updateAtomVelocitiesJH = new UpdateAtomVelocitiesJob { AtomVelocities = AtomVelocities }.Schedule(AtomQuery, doCollisionsJH);
        var updateCollisionStatsJH = new UpdateCollisionStatsJob { Collided = Collided }.Schedule(AtomQuery, doCollisionsJH);

        _hasRun = true;
        return JobHandle.CombineDependencies(updateAtomVelocitiesJH, updateCollisionStatsJH);
    }

    public void InitialiseMemory(int atomNumber)
    {
        Cleanup();
        Atoms = new NativeArray<Atom>(atomNumber, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        AtomVelocities = new NativeArray<Velocity>(atomNumber, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        BinnedAtoms = new NativeMultiHashMap<int, int>(atomNumber, Allocator.TempJob);
        UniqueBinIds = new NativeList<int>(atomNumber, Allocator.TempJob);
        Collided = new NativeArray<bool>(atomNumber, Allocator.TempJob, NativeArrayOptions.ClearMemory);
    }

    protected override void OnCreate()
    {
        Enabled = false;
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

    struct Atom
    {
        public Translation translation;
        public CollisionRadius radius;
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

    protected override void OnDestroy()
    {
        Cleanup();
    }

    /// <summary>
    /// Spatial Hashmap of Atoms. Key: bin, Values: atomId
    /// </summary>
    NativeMultiHashMap<int, int> BinnedAtoms;

    [BurstCompile]
    struct GetAtomDataJob : IJobForEachWithEntity<Translation, CollisionRadius, Mass, Velocity>
    {
        public NativeArray<Atom> Atoms;
        public NativeArray<Velocity> AtomVelocities;

        public void Execute(Entity entity, int index,
            [ReadOnly] ref Translation translation,
            [ReadOnly] ref CollisionRadius radius,
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
            for (int i = 0; i < length; i++)
                UniqueKeys.Add(keys[i]);
        }
    }

    [BurstCompile]
    struct DoCollisionsJob : IJobParallelFor
    {
        /// <summary>
        /// Time delta used for update
        /// </summary>
        public float dT;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<Atom> Atoms;
        [NativeDisableParallelForRestriction] public NativeArray<Velocity> AtomVelocities;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<int> BinIDs;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeMultiHashMap<int, int> BinnedAtoms;
        /// <summary>
        /// Set to true for atoms that have collided
        /// </summary>
        [NativeDisableParallelForRestriction] public NativeArray<bool> Collided;

        public void Execute2(int index)
        {
            if (index >= BinIDs.Length)
                return;
            int bin = BinIDs[index];
            DoMonteCarloCollisions(bin);
        }

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
                        Collide(outerID, innerID);
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


        /// <summary>
        /// Performs collisions using a Monte Carlo approach.
        /// 
        /// The collision rate is given as $n \sigma v$, where $n$ is the density, \sigma is the cross section and $v$ is the atom velocity.
        /// We define the cross section as 
        /// </summary>
        /// <param name="binID">bin within which to do collisions.</param>
        public void DoMonteCarloCollisions(int bin)
        {
            int i = 0;
            foreach (var atom1 in BinnedAtoms.GetValuesForKey(bin))
            {
                int j = 0;
                foreach (var atom2 in BinnedAtoms.GetValuesForKey(bin))
                {
                    if (i < j)
                        continue;

                    // Calculate the collision radius for the two hard spheres.
                    float collisionRadius = Atoms[atom1].radius.Value + Atoms[atom2].radius.Value;
                    float crossSection = math.PI * collisionRadius * collisionRadius;

                    // Determine the relative velocity between the two particles.
                    float3 relativeVelocity = AtomVelocities[atom1].Value - AtomVelocities[atom2].Value;

                    float collisionChance = dT * math.length(relativeVelocity) * crossSection / math.pow(COLLISION_CELL_SIZE, 3f);

                    // if (collisionChance > some random)
                    Collide(atom1, atom2);

                    j++;
                }
                i++;
            }
        }

        /// <summary>
        /// Ellastically collides two atoms.
        /// </summary>
        /// <param name="a">index of first atom</param>
        /// <param name="b">index of second atom</param>
        public void Collide(int a, int b)
        {
            // Velocity in center of mass frame
            float3 comv = (AtomVelocities[a].Value + AtomVelocities[b].Value) / 2f;

            //transform velocities to CoM frame
            float3 vel1 = AtomVelocities[a].Value - comv;
            float3 vel2 = AtomVelocities[b].Value - comv;

            //Only swap if velocities are facing each other in com frame
            if (math.dot(vel1, vel2) < 0f)
            {
                // swap velocities in CoM frame
                AtomVelocities[a] = new Velocity { Value = comv + vel2 };
                AtomVelocities[b] = new Velocity { Value = comv + vel1 };

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