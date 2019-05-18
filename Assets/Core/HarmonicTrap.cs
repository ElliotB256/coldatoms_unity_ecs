using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// The harmonic trap system applies a force to all trapped atoms, for each harmonic trap.
/// </summary>
[UpdateInGroup(typeof(ForceCalculationSystems))]
public class HarmonicTrapSystem : JobComponentSystem
{
    /// <summary>
    /// An entity query which selects all harmonic trap entities.
    /// </summary>
    EntityQuery HarmonicTrapQuery;

    /// <summary>
    /// An entity query which selects all trapped atoms.
    /// </summary>
    EntityQuery TrappedAtomQuery;

    struct Trap
    {
        public HarmonicTrap HarmonicTrap;
        public Translation Translation;
    }
    NativeArray<Trap> Traps;

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        // Get harmonic traps and load them into job memory.
        int trapCount = HarmonicTrapQuery.CalculateLength();
        Traps = new NativeArray<Trap>(trapCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        var getHarmonicTrapsJH = new GetHarmonicTrapsJob
        {
            Traps = Traps,
        }.Schedule(HarmonicTrapQuery, inputDependencies);

        var calculateHarmonicForcesJH = new CalculateHarmonicTrapForces
        {
            Traps = Traps,
        }.Schedule(TrappedAtomQuery, getHarmonicTrapsJH);

        return calculateHarmonicForcesJH;
    }

    [BurstCompile]
    struct GetHarmonicTrapsJob : IJobForEachWithEntity<Translation, HarmonicTrap>
    {
        public NativeArray<HarmonicTrap> Traps;
        public NativeArray<Translation> TrapPositions;

        public void Execute(Entity e, int i,
            [ReadOnly] ref Translation translation, [ReadOnly] ref HarmonicTrap trap)
        {
            Traps[i] = trap;
            TrapPositions[i] = translation;
        }
    }

    /// <summary>
    /// Calculates the forces applied to each atom by each harmonic trap.
    /// </summary>
    [BurstCompile]
    struct CalculateHarmonicTrapForces : IJobChunk
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<HarmonicTrap> Traps;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> TrapPositions;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> AtomPositions;
        public ArchetypeChunkComponentType<Force> AtomForces;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var atomPos = chunk.GetNativeArray(AtomPositions);
            var atomForces = chunk.GetNativeArray(AtomForces);

            for (int atomId = 0; atomId < atomForces.Length; atomId++)
            {
                var force = atomForces[atomId];
                for (int trapId = 0; trapId < Traps.Length; trapId++)
                {
                    force.Value += Traps[trapId].SpringConstant * (TrapPositions[trapId].Value - atomPos[atomId].Value);
                }
                atomForces[atomId] = force;
            }
        }
    }

    protected override void OnCreateManager()
    {
        TrappedAtomQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<Force>(),
                    ComponentType.ReadOnly<Trapped>()
                }
        }
        );

        HarmonicTrapQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<HarmonicTrap>()
                }
        }
        );
    }
}
