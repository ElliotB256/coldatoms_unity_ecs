using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Update velocity according to velocity verlet.
/// </summary>
[UpdateAfter(typeof(ForceCalculationSystems))]
public class UpdateVelocitySystem : JobComponentSystem
{
    [BurstCompile]
    struct UpdateTranslationJob : IJobForEach<Mass, Force, PrevForce, Velocity>
    {
        public float DeltaTime;

        public void Execute(
            [ReadOnly] ref Mass mass,
            [ReadOnly] ref Force force,
            [ReadOnly] ref PrevForce oldForce,
            ref Velocity velocity
            )
        {
            velocity.Value = velocity.Value + 0.5f * (force.Value + oldForce.Value) * DeltaTime / mass.Value;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new UpdateTranslationJob() { DeltaTime = Time.fixedDeltaTime };
        return job.Schedule(this, inputDependencies);
    }
}