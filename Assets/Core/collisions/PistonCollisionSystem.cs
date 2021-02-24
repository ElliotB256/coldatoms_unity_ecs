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
            // Don't need this query description as I am just finding the Piston entities to put in a nativearray
        // var query = new EntityQueryDesc
        // {
        //     None = new ComponentType[] { typeof(PrevForce)},
        //     All = new ComponentType[] {typeof(Piston), typeof(Translation), ComponentType.ReadOnly<Velocity>()}
        // };
        PistonQuery = GetEntityQuery(typeof(Piston));
    }

        // Do I need a job Handle if I am just doing this all at once?
        // Also need to make this run only over the bins that the wall is in
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        NativeArray<Piston> Pistons = PistonQuery.ToComponentDataArray<Piston>(Allocator.TempJob);
        // Debug.Log(Pistons.Length);
        return new UpdatePositionWithPistonJob
        {
            dT = DeltaTime,
            Pistons = Pistons

        }.Schedule(this, inputDependencies );
            // Should I use run here to just form the array?
                // I got an error "There is no boxing conversion from..."
    }

    [BurstCompile]
    [RequireComponentTag(typeof(Atom))]
    struct UpdatePositionWithPistonJob : IJobForEachWithEntity<Translation, Velocity, Mass, PrevForce>
    {
        public float dT;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Piston> Pistons;

        public void Execute(
            Entity entity,
            int index,
            ref Translation translation,
            ref Velocity velocity,
            [ReadOnly] ref Mass mass,
            [ReadOnly] ref PrevForce force)
        {
            // At the moment I am assuming that the Piston is only moving in the x direction and its normal is also in this direction.
            if (Pistons.Length == 0)
            {
                return;
            }
            
                // This loop is unnecessary as I only have one piston
            for (int i = 0; i < Pistons.Length; i ++) {
                    // This keeps the particles on the left 
                if (translation.Value.x > Pistons[i].Translation.x) {
                    
                    // Also need a check to see which zone the particles are in
                    // At the moment thi is just a wall assuming that all the particles are on the left 
                    // This collision model assuming the piston is an unstoppable force (infinite mass)

                    // This is leaking particles - slow push back in?
                    if (math.dot(velocity.Value, Pistons[i].Velocity) < 0f) {
                        velocity.Value.x = 2*Pistons[i].Velocity.x - velocity.Value.x;
                            // Change this WallCollisionDisplacement to positive for reverse collision
                        translation.Value.x = Pistons[i].Translation.x - 0.1f;
                    }

                        // This is a horrible quick and dirty way to get rid of the leaks to right of the piston
                    float catchmentDistance = 0.1f;
                    // float 

                    if (translation.Value.x > Pistons[i].Translation.x + catchmentDistance)
                    {
                        translation.Value.x = Pistons[i].Translation.x - 1f;
                    }
                }
            }
        }
    }
}   

// Why is this here and not in proxy?
    // I copied the format of the infinitePlane version so this applies to that also
    // It just seems it should be in the global namespace so putting it in a system that is revelant is ok?
/// <summary>
/// Represents a Piston
/// </summary>
[Serializable]
public struct Piston : IComponentData
{
    public float3 Translation;
    public float3 Velocity;
    public float Mass;
}