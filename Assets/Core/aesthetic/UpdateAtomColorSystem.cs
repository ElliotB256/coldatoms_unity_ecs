using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

[
    UpdateBefore(typeof(ForceCalculationSystems)),
    UpdateAfter(typeof(IncreaseCollisionTimerSystem)),
    AlwaysUpdateSystem
]
public class UpdateAtomColorSystem : JobComponentSystem
{
    [BurstCompile]
    struct UpdateAtomColorJob : IJobForEach<CollisionStats, ShaderCollisionTime>
    {
        public const float Test = 0.5f;

        public void Execute(
            [ReadOnly] ref CollisionStats stats,
            ref ShaderCollisionTime time)
        {
            time.Value = stats.TimeSinceLastCollision;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {   
        return new UpdateAtomColorJob().Schedule(this, inputDependencies);
    }
}