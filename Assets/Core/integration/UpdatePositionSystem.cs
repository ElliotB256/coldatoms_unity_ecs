using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Update position according to velocity verlet.
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
public class UpdatePositionSystem : JobComponentSystem
{
    [BurstCompile]
    struct UpdateTranslationJob : IJobForEach<Mass, PrevForce, Velocity, Translation>
    {
        public float DeltaTime;

        public void Execute(
            [ReadOnly] ref Mass mass,
            [ReadOnly] ref PrevForce force,
            [ReadOnly] ref Velocity velocity,
            ref Translation translation
            )
        {
            translation.Value = translation.Value + velocity.Value * DeltaTime + 0.5f * (force.Value / mass.Value) * DeltaTime * DeltaTime;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new UpdateTranslationJob() { DeltaTime = Time.fixedDeltaTime };
        return job.Schedule(this, inputDependencies);
    }
}