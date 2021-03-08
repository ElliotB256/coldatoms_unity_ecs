using Integration;
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
[UpdateAfter(typeof(UpdateVelocitySystem))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
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
        Masses = AtomQuery.ToComponentDataArrayAsync<Mass>(Allocator.TempJob, out var masJob);
        Translations = AtomQuery.ToComponentDataArrayAsync<Translation>(Allocator.TempJob, out var posJob);
        Radii = AtomQuery.ToComponentDataArrayAsync<CollisionRadius>(Allocator.TempJob, out var radJob);
        Velocities = AtomQuery.ToComponentDataArrayAsync<Velocity>(Allocator.TempJob, out var velJob);
        Zones = AtomQuery.ToComponentDataArrayAsync<Zone>(Allocator.TempJob, out var zoneJob);
        var getAtomData = JobHandle.CombineDependencies(JobHandle.CombineDependencies(masJob, posJob, radJob),zoneJob, velJob);

        var sortAtoms = new SortAtomsJob {
            BinnedAtoms = BinnedAtoms.AsParallelWriter(),
            BinIDs = BinIDs.AsParallelWriter(),
            CellSize = COLLISION_CELL_SIZE
        }.Schedule(AtomQuery, getAtomData);

        var getUniqueKeys = new GetFilledBoxIDs {
            //BinnedAtoms = BinnedAtoms,
            BinIds = BinIDs,
            UniqueKeys = UniqueBinIds
        }.Schedule(sortAtoms);

        var collisions = new DoCollisionsJob {
            BinIDs = UniqueBinIds,
            BinnedAtoms = BinnedAtoms,
            Translations = Translations,
            Masses = Masses,
            Radii = Radii,
            Velocities = Velocities,
            Zones = Zones,
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
        Zones.Dispose(updates);
        BinnedAtoms.Dispose(updates);
        UniqueBinIds.Dispose(updates);
        Collided.Dispose(updates);
        BinIDs.Dispose(updates);

        return updates;
    }

    public void InitialiseMemory(int atomNumber)
    {
        BinnedAtoms = new NativeMultiHashMap<int, int>(atomNumber, Allocator.TempJob);
        UniqueBinIds = new NativeList<int>(atomNumber, Allocator.TempJob);
        Collided = new NativeArray<bool>(atomNumber, Allocator.TempJob, NativeArrayOptions.ClearMemory);
        BinIDs = new NativeHashMap<int, bool>(atomNumber, Allocator.TempJob);
    }

    protected override void OnCreate()
    {
        AtomQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<CollisionRadius>(),
                    ComponentType.ReadOnly<Mass>(),
                    ComponentType.ReadOnly<Zone>(),
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
    NativeArray<Zone> Zones;
    NativeArray<Velocity> Velocities;
    NativeList<int> UniqueBinIds;
    NativeArray<bool> Collided;
    NativeHashMap<int, bool> BinIDs;

    /// <summary>
    /// Spatial Hashmap of Atoms. Key: bin, Values: atomId
    /// </summary>
    NativeMultiHashMap<int, int> BinnedAtoms;


    /// <summary>
    /// Sorts atoms into spatial hashmap
    /// </summary>
    [BurstCompile]
    struct SortAtomsJob : IJobForEachWithEntity<Translation>
    {
        [ReadOnly] public float CellSize;
        public NativeMultiHashMap<int, int>.ParallelWriter BinnedAtoms;
        public NativeHashMap<int, bool>.ParallelWriter BinIDs;

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation position)
        {
            int hash = (int)math.hash(new int3(math.floor(position.Value / CellSize)));
            BinnedAtoms.Add(hash, index);
            BinIDs.TryAdd(hash, true);
        }
    }

    /// <summary>
    /// Gets the ids of hashmap boxes that contain atoms.
    /// </summary>
    [BurstCompile]
    struct GetFilledBoxIDs : IJob
    {
        [ReadOnly] public NativeHashMap<int, bool> BinIds;
        public NativeList<int> UniqueKeys;

        public void Execute()
        {
            var keys = BinIds.GetKeyArray(Allocator.Temp);
            UniqueKeys.AddRange(keys);
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
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<Zone> Zones;
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
            // Add a check for particles in the same Zone
            if (Zones[a].Value == Zones[b].Value)
            {
                // Velocity in center of mass frame
                float3 vel1 = Velocities[a].Value;
                float3 vel2 = Velocities[b].Value;
            
                float mass1 = Masses[a].Value;
                float mass2 = Masses[b].Value;

                float3 comv = (mass1 * vel1 + mass2 * vel2) / (mass1 + mass2);

                //transform velocities to CoM frame
                vel1 -= comv;
                vel2 -= comv;

                //Only swap if velocities are facing each other in com frame
                if (math.dot(vel1, vel2) < 0f)
                {

                    float3 relativeSeparation = Translations[b].Value - Translations[a].Value;
                    float relativeSeparationMagSq = math.lengthsq(relativeSeparation);

                    // The positions of the particles only need to be proportional to this relativeSeparation as we only care about the sign.
                    // Only if both particles have negative dot product 
                    if (math.dot(vel1, relativeSeparation) > 0f & math.dot(vel2, relativeSeparation) < 0f)
                    {
                        // swap momenta in CoM frame
                            // Why not just use Particles[a] here instead of comv + vel1?
                        Velocities[a] = new Velocity { Value = comv + vel1 - (2/relativeSeparationMagSq)*math.dot(vel1, relativeSeparation)*relativeSeparation };
                        Velocities[b] = new Velocity { Value = comv + vel2 - (2/relativeSeparationMagSq)*math.dot(vel2, relativeSeparation)*relativeSeparation };
                        // Velocities[a] = new Velocity { Value = comv + vel2 * mass2 / mass1 };
                        // Velocities[b] = new Velocity { Value = comv + vel1 * mass1 / mass2 };

                        Collided[a] = true;
                        Collided[b] = true;
                 }
                }
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