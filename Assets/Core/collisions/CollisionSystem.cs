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
/// For fast performance, a HashMap Monte-Carlo method is used.
/// </summary>
public class CollisionSystem : JobComponentSystem
{
    public static float COLLISION_CELL_SIZE = 0.5f;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        int atomNumber = AtomQuery.CalculateLength();
        Atoms = new NativeArray<Atom>(atomNumber, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        AtomVelocities = new NativeArray<Velocity>(atomNumber, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        BinnedAtoms = new NativeMultiHashMap<int, int>(atomNumber, Allocator.TempJob);
        UniqueBinIds = new NativeList<int>(atomNumber, Allocator.TempJob);

        var getAtomDataJH = new GetAtomDataJob { Atoms = Atoms, AtomVelocities = AtomVelocities }.Schedule(AtomQuery, inputDeps);
        var sortAtomsJH = new SortAtomsJob { BinnedAtoms = BinnedAtoms.ToConcurrent(), CellSize = COLLISION_CELL_SIZE }.Schedule(AtomQuery, getAtomDataJH);
        var getUniqueKeysJH = new GetUniqueKeysJob { BinnedAtoms = BinnedAtoms, UniqueKeys = UniqueBinIds }.Schedule(sortAtomsJH);
        var doCollisionsJH = new DoCollisionsJob { Atoms = Atoms, AtomVelocities = AtomVelocities, BinIDs = UniqueBinIds, BinnedAtoms = BinnedAtoms }.Schedule(UniqueBinIds, 4, getUniqueKeysJH);
        var updateAtomVelocitiesJH = new UpdateAtomVelocitiesJob { AtomVelocities = AtomVelocities }.Schedule(AtomQuery, doCollisionsJH);
        var disposeJH = new DisposeJob { Atoms = Atoms, BinnedAtoms = BinnedAtoms, BinIDs = UniqueBinIds, AtomVelocities = AtomVelocities }.Schedule(updateAtomVelocitiesJH);

        return disposeJH;
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
                    ComponentType.ReadWrite<Velocity>()
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

    /// <summary>
    /// Spatial Hashmap of Atoms. Key: bin, Values: atomId
    /// </summary>
    NativeMultiHashMap<int, int> BinnedAtoms;

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
        public NativeMultiHashMap<int, int>.Concurrent BinnedAtoms;

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
            {
                UniqueKeys.Add(keys[i]);
            }
        }
    }

    struct DoCollisionsJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<Atom> Atoms;
        [NativeDisableParallelForRestriction] public NativeArray<Velocity> AtomVelocities;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeList<int> BinIDs;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeMultiHashMap<int, int> BinnedAtoms;

        /// <summary>
        /// Enumerate through each bin of the hash map, and collide atoms within that bin.
        /// </summary>
        /// <param name="index"></param>
        public void Execute(int index)
        {
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
            return math.lengthsq(Atoms[id1].translation.Value - Atoms[id2].translation.Value) < math.pow(Atoms[id1].radius.Value + Atoms[id2].radius.Value, 2f);
        }

        public void Collide(ref int id1, ref int id2)
        {
            // Velocity in center of mass frame
            float3 comv = (AtomVelocities[id1].Value + AtomVelocities[id2].Value) / 2f;

            //transform velocities to CoM frame
            float3 vel1 = AtomVelocities[id1].Value - comv;
            float3 vel2 = AtomVelocities[id2].Value - comv;

            // swap velocities in CoM frame
            AtomVelocities[id1] = new Velocity { Value = comv + vel2 };
            AtomVelocities[id2] = new Velocity { Value = comv + vel1 };
        }
    }

    struct UpdateAtomVelocitiesJob : IJobForEachWithEntity<Velocity>
    {
        public NativeArray<Velocity> AtomVelocities;

        public void Execute(Entity entity, int index, ref Velocity velocity)
        {
            velocity = AtomVelocities[index];
        }
    }

    struct DisposeJob : IJob
    {
        public NativeArray<Atom> Atoms;
        public NativeList<int> BinIDs;
        public NativeMultiHashMap<int, int> BinnedAtoms;
        public NativeArray<Velocity> AtomVelocities;

        public void Execute()
        {
            Atoms.Dispose();
            BinIDs.Dispose();
            BinnedAtoms.Dispose();
            AtomVelocities.Dispose();
        }
    }

}