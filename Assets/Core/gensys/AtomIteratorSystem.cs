using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// A generic system that updates entities (effectors) that enumerate over atoms
/// </summary>
public abstract class AtomIteratorSystem<TEffector, TAtomComponent, TProcess> : JobComponentSystem
    where TEffector : struct, IComponentData
    where TProcess : struct, IAtomIteratorStrategy<TEffector, TAtomComponent>
    where TAtomComponent : struct, IComponentData
{
    /// <summary>
    /// An entity query which selects all harmonic trap entities.
    /// </summary>
    protected EntityQuery AtomIteratorQuery;

    /// <summary>
    /// An entity query which selects all trapped atoms.
    /// </summary>
    protected EntityQuery AtomQuery;

    struct Effector
    {
        public TEffector Effect;
        public Translation Position;
    }
    NativeArray<Effector> Effectors;

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        int trapCount = AtomIteratorQuery.CalculateEntityCount();
        Effectors = new NativeArray<Effector>(trapCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        var getEffectorsJH = new GetEffectorsJob
        {
            Effectors = Effectors,
        }.Schedule(AtomIteratorQuery, inputDependencies);

        var enumerateAtomsJH = new EnumerateAtomsJob
        {
            Effectors = Effectors,
            AtomPositions = GetArchetypeChunkComponentType<Translation>(true),
            AtomComponents = GetArchetypeChunkComponentType<TAtomComponent>(false),
            IteratorStrategy = CreateIteratorStrategy()
        }.Schedule(AtomQuery, getEffectorsJH);

        return enumerateAtomsJH;
    }

    [BurstCompile]
    struct GetEffectorsJob : IJobForEachWithEntity<Translation, TEffector>
    {
        public NativeArray<Effector> Effectors;

        public void Execute(Entity e, int i,
            [ReadOnly] ref Translation translation, [ReadOnly] ref TEffector effect)
        {
            Effectors[i] = new Effector
            {
                Effect = effect,
                Position = translation
            };
        }
    }

    /// <summary>
    /// Calculates the forces applied to each atom by each harmonic trap.
    /// </summary>
    [BurstCompile]
    struct EnumerateAtomsJob : IJobChunk
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Effector> Effectors;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> AtomPositions;
        public ArchetypeChunkComponentType<TAtomComponent> AtomComponents;
        public TProcess IteratorStrategy;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var atomPos = chunk.GetNativeArray(AtomPositions);
            var atomComps = chunk.GetNativeArray(AtomComponents);

            for (int atomId = 0; atomId < atomComps.Length; atomId++)
            {
                var atomComponent = atomComps[atomId];
                for (int trapId = 0; trapId < Effectors.Length; trapId++)
                {
                    //IteratorStrategy.Execute(Effectors[trapId], ref atomComponent);
                }
                atomComps[atomId] = atomComponent;
            }
        }
    }

    protected abstract TProcess CreateIteratorStrategy();

    protected override void OnCreateManager()
    {
        AtomQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<Force>(),
                    ComponentType.ReadOnly<Trapped>()
                }
        }
        );

        AtomIteratorQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<HarmonicTrap>()
                }
        }
        );
    }
}
