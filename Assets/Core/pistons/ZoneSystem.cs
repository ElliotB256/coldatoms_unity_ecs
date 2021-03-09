using Integration;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
// only using this for the Debug.Log()
using UnityEngine;

/// <summary>
/// Determine which Zone the particles are in 
/// </summary>

[AlwaysUpdateSystem]
[UpdateBefore(typeof(ForceCalculationSystems))]
    // Update this before PistonCollisionSystem
        // Or maybe at the end? Otherwise it may mess up the Piston Collisions
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class ZoneSystem : JobComponentSystem
{
    EntityQuery PistonQuery;
    EntityQuery DiaphragmQuery;

    protected override void OnCreate()
    {   
            // Defining the Query for the piston
        var pistonQueryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Piston>()
            }
        };
        PistonQuery = GetEntityQuery(pistonQueryDesc);

        var diaphragmQueryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Diaphragm>()
            }
        };
        DiaphragmQuery = GetEntityQuery(diaphragmQueryDesc);
    }

        // Do I need a job Handle if I am just doing this all at once?
        // Also need to make this run only over the bins that the wall is in
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        NativeArray<Translation> PistonTranslation = PistonQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<Translation> DiaphragmTranslation = DiaphragmQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

        return new UpdatePositionWithPistonJob
        {
            dT = DeltaTime,
            pistonTranslation = PistonTranslation,
            diaphragmTranslation = DiaphragmTranslation
        }.Schedule(this, inputDependencies );
    }

    

    [BurstCompile]
    [RequireComponentTag(typeof(Atom))]
    struct UpdatePositionWithPistonJob : IJobForEachWithEntity<Translation, Zone>
    {
        public float dT;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Translation> pistonTranslation;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Translation> diaphragmTranslation;

        public void Execute(
            Entity entity,
            int index,
            [ReadOnly] ref Translation translation,
            ref Zone zone
            )
        {
            // At the moment I am assuming that the Piston is only moving in the x direction and its normal is also in this direction.
            if (pistonTranslation.Length + diaphragmTranslation.Length == 0)
            {
                // Setting this value means the particles will not attempt to find a zone  
                zone.Value = -2;
                return;
            }

            if (zone.Value == -1)
            {
                if (diaphragmTranslation.Length != 0 && translation.Value.x > diaphragmTranslation[0].Value.x) {
                    zone.Value = 2;             
                }
                // At the moment everything assumes a piston and a diaphram
                else if (pistonTranslation.Length != 0 && translation.Value.x > pistonTranslation[0].Value.x)
                {
                    zone.Value = 1;
                }
                else
                {
                    zone.Value = 0;
                }
            }
        }
    }
}   