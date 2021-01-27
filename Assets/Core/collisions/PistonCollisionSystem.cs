using Integration;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

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
        PistonQuery = GetEntityQuery(typeof(Piston));
    }

        // Do I need a job Handle if I am just doing this all at once?
        // Also need to make this run only over the bins that the wall is in
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        NativeArray<Piston> Pistons = PistonQuery.ToComponentDataArray<Piston>(Allocator.TempJob);
        return new UpdatePositionWithPistonJob
        {
            dT = DeltaTime,
            Pistons = Pistons
        }.Schedule(this, inputDependencies );
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
                if (translation.Value.x > Pistons[i].Position.x) {

                    if (math.dot(velocity.Value, Pistons[i].Velocity) < 0f) {
                        velocity.Value.x = 2*Pistons[i].Velocity.x - velocity.Value.x;
                    }
                }
            }
        }
    }
}


// Why is this here and not in proxy?
    // I copied the format of the infinitePlane version so this applies to that also
/// <summary>
/// Represents a triangular segment of wall that atoms can deflect off.
    // Not sure what triangular means here?
/// </summary>
[Serializable]
public struct Piston : IComponentData
{
    public float3 Position;
    public float3 Velocity;
    public float Mass;
}