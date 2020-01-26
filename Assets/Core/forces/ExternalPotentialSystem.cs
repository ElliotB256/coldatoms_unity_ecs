using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Forces {

    public interface ICalculator<T>
    {
        float3 CalculateForce(in T potential, in LocalToWorld potentialLocation, in LocalToWorld atomLocation);
        float CalculatePotential(in T potential, in LocalToWorld potentialLocation, in LocalToWorld atomLocation);
    }

    /// <summary>
    /// Performs force and potential calculations for external force systems.
    /// </summary>
    [UpdateInGroup(typeof(ForceCalculationSystems))]
    public abstract class ExternalPotentialSystem<TPotential, Calculator> : JobComponentSystem
        where TPotential : struct, IComponentData
        where Calculator : struct, ICalculator<TPotential>
    {
        /// <summary>
        /// Returns the Calculator used to calculate force and potential energy.
        /// </summary>
        /// <returns></returns>
        protected abstract Calculator GetCalculator();

        /// <summary>
        /// An entity query which selects all potentials.
        /// </summary>
        EntityQuery PotentialQuery;

        /// <summary>
        /// An entity query which selects all trapped atoms.
        /// </summary>
        EntityQuery AtomQuery;

        struct ExternalPotential
        {
            public TPotential Potential;
            public LocalToWorld Transform;
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            NativeArray<ExternalPotential> Potentials = new NativeArray<ExternalPotential>(
                PotentialQuery.CalculateEntityCount(),
                Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory
                );

            var getPotentials = Entities
                .ForEach(
                (int entityInQueryIndex, in LocalToWorld transform, in TPotential potential) =>
                    Potentials[entityInQueryIndex] = new ExternalPotential { Potential = potential, Transform = transform }
                )
                .WithNativeDisableParallelForRestriction(Potentials)
                .Schedule(inputDependencies);

            var applyPotentials = new ApplyPotentials
            {
                Potentials = Potentials,
                AtomTransforms = GetArchetypeChunkComponentType<LocalToWorld>(true),
                AtomForces = GetArchetypeChunkComponentType<Force>(false)
            }.Schedule(AtomQuery, getPotentials);

            return applyPotentials;
        }

        /// <summary>
        /// Calculates the forces applied to each atom by each harmonic trap.
        /// </summary>
        [BurstCompile]
        struct ApplyPotentials : IJobChunk
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<ExternalPotential> Potentials;
            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> AtomTransforms;
            public ArchetypeChunkComponentType<Force> AtomForces;
            public Calculator Calculator;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var atomPos = chunk.GetNativeArray(AtomTransforms);
                var atomForces = chunk.GetNativeArray(AtomForces);

                for (int atomId = 0; atomId < atomForces.Length; atomId++)
                {
                    var force = atomForces[atomId];
                    for (int trapId = 0; trapId < Potentials.Length; trapId++)
                    {
                        force.Value += Calculator.CalculateForce(Potentials[trapId].Potential, Potentials[trapId].Transform, atomPos[atomId]);
                    }
                    atomForces[atomId] = force;
                }
            }
        }

        protected override void OnCreateManager()
        {
            AtomQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadOnly<Force>(),
                    ComponentType.ReadOnly<Trapped>()
                }
            }
            );

            PotentialQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadOnly<TPotential>()
                }
            }
            );
        }
    }
}