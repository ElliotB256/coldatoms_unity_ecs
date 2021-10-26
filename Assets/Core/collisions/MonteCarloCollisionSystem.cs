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
public class MonteCarloCollisionSystem : SystemBase
{
    /// <summary>
    /// Size of a collision cell, in Unity units.
    /// </summary>
    public static float COLLISION_CELL_SIZE = 2f;

    protected override void OnUpdate()
    {
        int atomNumber = AtomQuery.CalculateEntityCount();
        var atoms = new NativeArray<Atom>(atomNumber, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var binnedAtoms = new NativeMultiHashMap<int, int>(atomNumber, Allocator.TempJob);
        var uniqueBinIds = new NativeList<int>(atomNumber, Allocator.TempJob);
        var collided = new NativeArray<bool>(atomNumber, Allocator.TempJob, NativeArrayOptions.ClearMemory);

        // Fill atom arrays.
        Dependency = Entities
            .WithAll<CollisionStats, Trapped>()
            .WithStoreEntityQueryInField(ref AtomQuery)
            .ForEach(
                (
                    Entity e, int entityInQueryIndex,
                    ref Translation translation,
                    ref CollisionRadius radius,
                    ref Mass mass,
                    ref Velocity velocity
                    ) =>
                {
                    atoms[entityInQueryIndex] = new Atom { translation = translation, radius = radius, mass = mass, velocity = velocity };
                }
            )
            .Schedule(Dependency);

        // Spatially sort atoms to produce hashmap.
        Dependency = new SortAtomsJob {
            Atoms = atoms,
            BinnedAtoms = binnedAtoms.AsParallelWriter(),
            CellSize = COLLISION_CELL_SIZE
        }.Schedule(atomNumber, Dependency);

        Dependency = new GetUniqueKeysJob { BinnedAtoms = binnedAtoms, UniqueKeys = uniqueBinIds }.Schedule(Dependency);
        Dependency = new DoCollisionsJob { 
            dT = FixedUpdateGroup.FixedTimeDelta,
            Atoms = atoms,
            BinIDs = uniqueBinIds,
            BinnedAtoms = binnedAtoms,
            Collided = collided,
            Random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 10000))
    }.Schedule(atomNumber, 1, Dependency);
        
        // Update atom velocities with collided values.
        var updateAtomVelocitiesJH = new UpdateAtomVelocitiesJob {
            Atoms = atoms,
            velocityTypeHandle = GetComponentTypeHandle<Velocity>(false)
        }.Schedule(AtomQuery, Dependency);

        var updateCollisionStatsJH = new UpdateCollisionStatsJob {
            Collided = collided,
            statsTypeHandle = GetComponentTypeHandle<CollisionStats>(false)
        }.Schedule(AtomQuery, Dependency);

        Dependency = JobHandle.CombineDependencies(updateAtomVelocitiesJH, updateCollisionStatsJH);

        atoms.Dispose(Dependency);
        uniqueBinIds.Dispose(Dependency);
        binnedAtoms.Dispose(Dependency);
        collided.Dispose(Dependency);
    }

    EntityQuery AtomQuery;

    struct Atom
    {
        public Translation translation;
        public CollisionRadius radius;
        public Mass mass;
        public Velocity velocity;
    }

    /// <summary>
    /// Sorts atoms into spatial hashmap
    /// </summary>
    [BurstCompile]
    struct SortAtomsJob : IJobFor
    {
        [ReadOnly] public float CellSize;
        public NativeArray<Atom> Atoms;
        public NativeMultiHashMap<int, int>.ParallelWriter BinnedAtoms;

        public void Execute(int index)
        {
            int hash = (int)math.hash(new int3(math.floor(Atoms[index].translation.Value / CellSize)));
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
        [NativeDisableParallelForRestriction] public NativeArray<Atom> Atoms;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<int> BinIDs;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeMultiHashMap<int, int> BinnedAtoms;
        /// <summary>
        /// Set to true for atoms that have collided
        /// </summary>
        [NativeDisableParallelForRestriction] public NativeArray<bool> Collided;
        public Unity.Mathematics.Random Random;

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
            var occupancy = BinnedAtoms.CountValuesForKey(bin);
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
                    float3 relativeVelocity = Atoms[atom1].velocity.Value - Atoms[atom2].velocity.Value;
                    var n = 1 / math.pow(COLLISION_CELL_SIZE, 3f);
                    float collisionChance = dT * math.length(relativeVelocity) * crossSection * n;

                    if (collisionChance > Random.NextFloat())
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
            var atomA = Atoms[a];
            var atomB = Atoms[b];
            // Velocity in center of mass frame
            float3 comv = (atomA.velocity.Value + atomB.velocity.Value) / 2f;

            //transform velocities to CoM frame
            float3 vel1 = atomA.velocity.Value - comv;
            float3 vel2 = atomB.velocity.Value - comv;

            //Only swap if velocities are facing each other in com frame
            if (math.dot(vel1, vel2) < 0f)
            {
                // swap velocities in CoM frame
                atomA.velocity = new Velocity { Value = comv + vel2 };
                atomB.velocity = new Velocity { Value = comv + vel1 };

                Atoms[a] = atomA;
                Atoms[b] = atomB;

                Collided[a] = true;
                Collided[b] = true;
            }
        }
    }

    [BurstCompile]
    struct UpdateAtomVelocitiesJob : IJobEntityBatchWithIndex
    {
        public NativeArray<Atom> Atoms;
        public ComponentTypeHandle<Velocity> velocityTypeHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery)
        {
            NativeArray<Velocity> velocities = batchInChunk.GetNativeArray(velocityTypeHandle);

            for (int i = 0; i < batchInChunk.Count; i++)
                velocities[i] = Atoms[i+indexOfFirstEntityInQuery].velocity;
        }
    }

    [BurstCompile]
    struct UpdateCollisionStatsJob : IJobEntityBatchWithIndex
    {
        public NativeArray<bool> Collided;
        public ComponentTypeHandle<CollisionStats> statsTypeHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery)
        {
            NativeArray<CollisionStats> stats = batchInChunk.GetNativeArray(statsTypeHandle);
            
            for (int i=0; i<batchInChunk.Count; i++)
                if (Collided[i+indexOfFirstEntityInQuery])
                {
                    var stat = stats[i];
                    stat.TimeSinceLastCollision = 0f;
                    stats[i] = stat;
                }
        }
    }
}