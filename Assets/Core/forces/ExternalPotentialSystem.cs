using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Forces
{
    public interface ICalculator<TPotential, TComp>
    {
        float3 CalculateForce(in TPotential potential, in Translation potentialLocation, in Translation atomLocation, in TComp atomComponent);
        float CalculatePotential(in TPotential potential, in Translation potentialLocation, in Translation atomLocation, in TComp atomComponent);
    }

    /// <summary>
    /// Performs force and potential calculations for external force systems.
    /// </summary>
    [UpdateInGroup(typeof(ForceCalculationSystems))]
    public abstract class ExternalPotentialSystem<TPotential, TComponent, TCalculator> : JobComponentSystem
        where TPotential : struct, IComponentData
        where TComponent : struct, IComponentData
        where TCalculator : struct, ICalculator<TPotential, TComponent>
    {
        /// <summary>
        /// Returns the Calculator used to calculate force and potential energy.
        /// </summary>
        /// <returns></returns>
        protected abstract TCalculator GetCalculator();

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
            NativeArray<TPotential> potentials = PotentialQuery.ToComponentDataArray<TPotential>(Allocator.TempJob);
            NativeArray<Translation> potentialPos = PotentialQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

            var applyPotentials = new ApplyPotentialsJob<TPotential, TComponent, TCalculator>
            {
                Potentials = potentials,
                PotentialPos = potentialPos,
                AtomPos = GetComponentTypeHandle<Translation>(true),
                AtomComponent = GetComponentTypeHandle<TComponent>(true),
                AtomForces = GetComponentTypeHandle<Force>(false),
                AtomPEs = GetComponentTypeHandle<PotentialEnergy>(false),
            }.Schedule(AtomQuery, inputDependencies);

            return applyPotentials;
        }

        protected override void OnCreate()
        {
            AtomQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<Force>(),
                    ComponentType.ReadOnly<Trapped>(),
                    ComponentType.ReadOnly<TComponent>()
                }
            }
            );

            PotentialQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<TPotential>()
                }
            }
            );
        }
    }

    /// <summary>
    /// Calculates the forces applied to each atom by each harmonic trap.
    /// </summary>
    [BurstCompile]
    public struct ApplyPotentialsJob<TPotential, TComponent, TCalculator> : IJobChunk
        where TPotential : struct, IComponentData
        where TComponent : struct, IComponentData
        where TCalculator : struct, ICalculator<TPotential, TComponent>
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<TPotential> Potentials;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> PotentialPos;
        [ReadOnly] public ComponentTypeHandle<Translation> AtomPos;
        [ReadOnly] public ComponentTypeHandle<TComponent> AtomComponent;
        public ComponentTypeHandle<Force> AtomForces;
        public ComponentTypeHandle<PotentialEnergy> AtomPEs;
        public TCalculator Calculator;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var atomPos = chunk.GetNativeArray(AtomPos);
            var atomForces = chunk.GetNativeArray(AtomForces);
            var atomPEs = chunk.GetNativeArray(AtomPEs);
            var atomComponent = chunk.GetNativeArray(AtomComponent);

            for (int atomId = 0; atomId < atomForces.Length; atomId++)
            {
                var force = atomForces[atomId];
                var pe = atomPEs[atomId];
                for (int trapId = 0; trapId < Potentials.Length; trapId++)
                {
                    force.Value += Calculator.CalculateForce(Potentials[trapId], PotentialPos[trapId], atomPos[atomId], atomComponent[atomId]);
                    pe.Value += Calculator.CalculatePotential(Potentials[trapId], PotentialPos[trapId], atomPos[atomId], atomComponent[atomId]);
                }
                atomForces[atomId] = force;
                atomPEs[atomId] = pe;
            }
        }
    }
}