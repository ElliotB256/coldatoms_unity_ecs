using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(ForceCalculationSystems))]
public class DoubleWellTrapSystem : JobComponentSystem
{
    /// <summary>
    /// An entity query which selects all harmonic trap entities.
    /// </summary>
    EntityQuery DoubleWellTrapQuery;

    /// <summary>
    /// An entity query which selects all trapped atoms.
    /// </summary>
    EntityQuery TrappedAtomQuery;

    struct Trap
    {
        public DoubleWellTrap DoubleWell;
        public Translation Position;
    }
    NativeArray<Trap> Traps;

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        // Get harmonic traps and load them into job memory.
        int trapCount = DoubleWellTrapQuery.CalculateLength();
        Traps = new NativeArray<Trap>(trapCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        var getHarmonicTrapsJH = new GetDoubleWellsJob
        {
            Traps = Traps,
        }.Schedule(DoubleWellTrapQuery, inputDependencies);

        var calculateHarmonicForcesJH = new CalculateDoubleWellTrapForces
        {
            Traps = Traps,
            AtomPositions = GetArchetypeChunkComponentType<Translation>(true),
            AtomForces = GetArchetypeChunkComponentType<Force>(false)
        }.Schedule(TrappedAtomQuery, getHarmonicTrapsJH);

        return calculateHarmonicForcesJH;
    }

    [BurstCompile]
    struct GetDoubleWellsJob : IJobForEachWithEntity<Translation, DoubleWellTrap>
    {
        public NativeArray<Trap> Traps;

        public void Execute(Entity e, int i,
            [ReadOnly] ref Translation translation, [ReadOnly] ref DoubleWellTrap trap)
        {
            Traps[i] = new Trap
            {
                DoubleWell = trap,
                Position = translation
            };
        }
    }

    /// <summary>
    /// Calculates the forces applied to each atom by each harmonic trap.
    /// </summary>
    [BurstCompile]
    struct CalculateDoubleWellTrapForces : IJobChunk
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Trap> Traps;
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
                    // Exert harmonic confinement along two directions, and the double-well trap force along the other.
                    float x = Traps[trapId].Position.Value.x - atomPos[atomId].Value.x;
                    x = math.sign(x) * math.max(0f, math.abs(x) - Traps[trapId].DoubleWell.Separation);
                    float3 trapForce = new float3(
                        -Traps[trapId].DoubleWell.SpringConstant * 2 * x + Traps[trapId].DoubleWell.alpha * math.pow(x,3f),
                        Traps[trapId].DoubleWell.SpringConstant * (Traps[trapId].Position.Value.y - atomPos[atomId].Value.y),
                        Traps[trapId].DoubleWell.SpringConstant * (Traps[trapId].Position.Value.z - atomPos[atomId].Value.z)
                    );

                    force.Value += trapForce;
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

        DoubleWellTrapQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<DoubleWellTrap>()
                }
        }
        );
    }
}
