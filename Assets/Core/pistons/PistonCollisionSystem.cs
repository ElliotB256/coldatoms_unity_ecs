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
/// Cause collisions between particles and moving piston.
/// </summary>
[AlwaysUpdateSystem]
[UpdateBefore(typeof(ForceCalculationSystems))]
// How can I make this update just after the wall system? [UpdateAfter ..] isn't a thing
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class PistonCollisionSystem : JobComponentSystem
{
    EntityQuery PistonQuery;

    protected override void OnCreate()
    {   
            // Defining the Query for the piston
        var query = new EntityQueryDesc
        {
            All = new ComponentType[] {
                typeof(Translation),
                ComponentType.ReadOnly<Velocity>(),
                ComponentType.ReadOnly<Mass>(),
                ComponentType.ReadOnly<Piston>()
            }
        };
        PistonQuery = GetEntityQuery(query);
    }

        // Do I need a job Handle if I am just doing this all at once?
        // Also need to make this run only over the bins that the wall is in
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        NativeArray<Translation> PistonTranslation = PistonQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<Velocity> PistonVelocity = PistonQuery.ToComponentDataArray<Velocity>(Allocator.TempJob);
        NativeArray<Mass> PistonMass = PistonQuery.ToComponentDataArray<Mass>(Allocator.TempJob);

        return new UpdatePositionWithPistonJob
        {
            dT = DeltaTime,
            pistonTranslation = PistonTranslation,
            pistonVelocity = PistonVelocity,
            pistonMass = PistonMass

        }.Schedule(this, inputDependencies );
            // Should I use run here to just form the array?
                // I got an error "There is no boxing conversion from..."
    }

    

    [BurstCompile]
    [RequireComponentTag(typeof(Atom))]
        // Why can't I include Atom in this list? 
            // Is it because I am not doing anything with it?
    struct UpdatePositionWithPistonJob : IJobForEachWithEntity<Translation, Velocity, Mass, Zone>
    {
        public float dT;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Translation> pistonTranslation;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Velocity> pistonVelocity;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Mass> pistonMass;

        public void Execute(
            Entity entity,
            int index,
            ref Translation translation,
            ref Velocity velocity,
            [ReadOnly] ref Mass mass,
            [ReadOnly] ref Zone zone
            )
        {
            // At the moment I am assuming that the Piston is only moving in the x direction and its normal is also in this direction.
            if (pistonTranslation.Length == 0 || zone.Value == 3)
            {
                return;
            }

            float wallDisplacementDistance =  0.1f;
            
                // This loop is unnecessary as I only have one piston
            for (int i = 0; i < pistonTranslation.Length; i ++) {

                if (zone.Value == 0)
                { 
                    if (translation.Value.x > pistonTranslation[i].Value.x) {
                            //Change these from vectors to just the scalar x value
                        float3 CoMVelocity = (pistonMass[i].Value * pistonVelocity[i].Value + mass.Value*velocity.Value)/(pistonMass[i].Value + mass.Value);
                        float3 particleCoMVelocity = velocity.Value - CoMVelocity;
                        float3 pistonCoMVelocity = pistonVelocity[i].Value - CoMVelocity;
                        
                        // This collision model assuming the piston is an unstoppable force (infinite mass)
                        if (math.dot(particleCoMVelocity, pistonCoMVelocity) < 0f) {
                            // particleCoMVelocity.x = 2*pistonCoMVelocity.x - particleCoMVelocity.x;
                            // translation.Value.x = Pistons[i].Translation.x - wallDisplacementDistance;
                            velocity.Value.x = 2*pistonVelocity[i].Value.x - velocity.Value.x;
                        }
                    }
                }
                    // remove this last or when the diaphragm is not in place
                if (zone.Value == 1 || zone.Value == 2)
                {
                    if (translation.Value.x < pistonTranslation[i].Value.x) {

                        float3 CoMVelocity = (pistonMass[i].Value * pistonVelocity[i].Value + mass.Value*velocity.Value)/(pistonMass[i].Value + mass.Value);
                        float3 particleCoMVelocity = velocity.Value - CoMVelocity;
                        float3 pistonCoMVelocity = pistonVelocity[i].Value - CoMVelocity;
                        
                        // This collision model assuming the piston is an unstoppable force (infinite mass)
                        if (math.dot(particleCoMVelocity, pistonCoMVelocity) < 0f) {
                            // particleCoMVelocity.x = 2*pistonCoMVelocity.x - particleCoMVelocity.x;
                            // translation.Value.x = Pistons[i].Translation.x - wallDisplacementDistance;
                            velocity.Value.x = 2*pistonVelocity[i].Value.x - velocity.Value.x;                        }
                    }

                }





                // This doesn't work and I don't know why.
                //  if (zone.Value == 0)
                //  { 
                //     if (translation.Value.x > pistonTranslation[i].Value.x) {

                //         //translation.Value.x = pistonTranslation[i].Value.x - wallDisplacementDistance;

                //             // Check the particle is moving towards the wall
                //                 // Change this to check that the sign of the x component is opposite
                                    // Can just check that the com particle velocity is positive
                //         if (math.dot(velocity.Value - pistonVelocity[i].Value, pistonVelocity[i].Value) < 0f) {
                //                 // This collision model assuming the piston is an unstoppable force (infinite mass)
                //             velocity.Value.x = 2*pistonVelocity[i].Value.x - velocity.Value.x;
                //         }
                //     }
                // }
                // if (zone.Value == 1)
                // {
                //     if (translation.Value.x < pistonTranslation[i].Value.x) {
                        
                //         //translation.Value.x = pistonTranslation[i].Value.x - wallDisplacementDistance;
                //             // Check that they are moving towards each other in the Piston Frame
                //         if (math.dot(velocity.Value - pistonVelocity[i].Value, pistonVelocity[i].Value) < 0f) {
                //             velocity.Value.x = 2*pistonVelocity[i].Value.x - velocity.Value.x;
                //         }
                //     }
                // }
            }
        }
    }
}   