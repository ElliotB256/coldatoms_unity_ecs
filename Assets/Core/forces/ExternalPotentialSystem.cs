﻿using Unity.Burst;
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
    public abstract class ExternalPotentialSystem<TPotential, TComponent, Calculator> : JobComponentSystem
        where TPotential : struct, IComponentData
        where TComponent : struct, IComponentData
        where Calculator : struct, ICalculator<TPotential, TComponent>
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
            NativeArray<TPotential> potentials = PotentialQuery.ToComponentDataArray<TPotential>(Allocator.TempJob);
            NativeArray<Translation> potentialPos = PotentialQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

            var applyPotentials = new ApplyPotentials
            {
                Potentials = potentials,
                PotentialPos = potentialPos,
                AtomPos = GetArchetypeChunkComponentType<Translation>(true),
                AtomComponent = GetArchetypeChunkComponentType<TComponent>(true),
                AtomForces = GetArchetypeChunkComponentType<Force>(false),
                AtomPEs = GetArchetypeChunkComponentType<PotentialEnergy>(false),
            }.Schedule(AtomQuery, inputDependencies);

            return applyPotentials;
        }

        /// <summary>
        /// Calculates the forces applied to each atom by each harmonic trap.
        /// </summary>
        [BurstCompile]
        struct ApplyPotentials : IJobChunk
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<TPotential> Potentials;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> PotentialPos;
            [ReadOnly] public ArchetypeChunkComponentType<Translation> AtomPos;
            [ReadOnly] public ArchetypeChunkComponentType<TComponent> AtomComponent;
            public ArchetypeChunkComponentType<Force> AtomForces;
            public ArchetypeChunkComponentType<PotentialEnergy> AtomPEs;
            public Calculator Calculator;

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
}