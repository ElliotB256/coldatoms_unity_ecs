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
    EntityQuery AtomQuery;
    [DeallocateOnJobCompletion] NativeArray<bool> Collided;
    // static float holeSize = 0.1f;

    protected override void OnCreate()
    {   

        Enabled = true;
            // Defining the Query for the piston
        var query = new EntityQueryDesc
        {
            All = new ComponentType[] {
                typeof(Translation),
                ComponentType.ReadOnly<Velocity>(),
                ComponentType.ReadOnly<Mass>(),
                ComponentType.ReadOnly<Piston>(),
                ComponentType.ReadOnly<WIndex>()
            }
        };
        PistonQuery = GetEntityQuery(query);

        AtomQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]  {
                typeof(CollisionStats),
                ComponentType.ReadOnly<Atom>()
            }
        });
    }

        // Do I need a job Handle if I am just doing this all at once?
        // Also need to make this run only over the bins that the wall is in
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
            // Form an array for Collided particles
        int atomNumber = AtomQuery.CalculateEntityCount();
        Collided = new NativeArray<bool>(atomNumber, Allocator.TempJob, NativeArrayOptions.ClearMemory);

        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        NativeArray<Translation> PistonTranslation = PistonQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<Velocity> PistonVelocity = PistonQuery.ToComponentDataArray<Velocity>(Allocator.TempJob);
        NativeArray<Mass> PistonMass = PistonQuery.ToComponentDataArray<Mass>(Allocator.TempJob);
        NativeArray<WIndex> PistonWIndex = PistonQuery.ToComponentDataArray<WIndex>(Allocator.TempJob);

        var firstJob = new UpdatePositionWithPistonJob
        {
            dT = DeltaTime,
            pistonTranslation = PistonTranslation,
            pistonVelocity = PistonVelocity,
            pistonMass = PistonMass,
            pistonWIndex = PistonWIndex,
            Collided = Collided
        }.Schedule(this, inputDependencies );

        return new UpdateCollisionStatsJob
        {
            Collided = Collided
        }.Schedule(AtomQuery, firstJob);

    }

    

    [BurstCompile]
    [RequireComponentTag(typeof(Atom))]
        // Why can't I include Atom in this list? 
            // Is it because I am not doing anything with it?
    struct UpdatePositionWithPistonJob : IJobForEachWithEntity<Translation, Velocity, WallCollisions, Zone, Mass, CollisionRadius>
    {
        public float dT;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Translation> pistonTranslation;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Velocity> pistonVelocity;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Mass> pistonMass;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<WIndex> pistonWIndex;
        public NativeArray<bool> Collided;

        public void Execute(
            Entity entity,
            int index,
            ref Translation translation,
            ref Velocity velocity,
            ref WallCollisions wallCollisions,
            ref Zone zone,
            [ReadOnly] ref Mass mass,
            [ReadOnly] ref CollisionRadius radius
            )
        {
            // At the moment I am assuming that the Piston is only moving in the x direction and its normal is also in this direction.
            if (pistonTranslation.Length == 0 || zone.Value == 3)
            {
                return;
            }
            
                // This loop is unnecessary as I only have one piston
            for (int i = 0; i < pistonTranslation.Length; i ++) {


                            // Do an effusion system rather than this
                        // if ( translation.Value.y > -holeSize && translation.Value.y < holeSize && translation.Value.z > -holeSize && translation.Value.z < holeSize)
                        // {
                        //     if (velocity.Value.x > 0)
                        //     {
                        //             // Effusion
                        //         zone.Value = 1;
                        //     }
                        // }
                        // else {
                        //    // Normal Wall Collision

                        // }

                 if (zone.Value == 0)
                 { 
                    if (translation.Value.x > pistonTranslation[i].Value.x - radius.Value) {

                        float relVelX = velocity.Value.x - pistonVelocity[i].Value.x;
                        
                        if (relVelX > 0)
                        {
                            // Collide

                            velocity.Value.x = 2*pistonVelocity[i].Value.x - velocity.Value.x;
                            Collided[index] = true;
                        }
                    }
                }
                if (zone.Value == 1)
                {
                    if (translation.Value.x < pistonTranslation[i].Value.x + radius.Value) {

                        float relVelX = velocity.Value.x - pistonVelocity[i].Value.x;
                        
                        if (relVelX < 0)
                        {
                            // Collide

                            velocity.Value.x = 2*pistonVelocity[i].Value.x - velocity.Value.x;
                            Collided[index] = true;
                        }
                    }
                }
            }
        }
    }

            // Job to update the collision statistics 
    [BurstCompile]
    struct UpdateCollisionStatsJob : IJobForEachWithEntity<CollisionStats>
    {
        public NativeArray<bool> Collided;

        public void Execute(Entity entity, int index, ref CollisionStats stats)
        {
            if (Collided[index])
                // stats.TimeSinceLastCollision = 0f;
                stats.CollidedThisFrame = true;
        }
    }
}   