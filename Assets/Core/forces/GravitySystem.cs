﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// Applies gravitational force to all entities with mass.
/// </summary>
[UpdateInGroup(typeof(ForceCalculationSystems))]
public class GravitySystem : JobComponentSystem
{
    public const float GRAVITATIONAL_CONSTANT = 20.0f;

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return Entities
                .WithNone<Trapped>()
                .ForEach(
                    (ref Force force, in Mass mass)
                        => force.Value = force.Value - mass.Value * GRAVITATIONAL_CONSTANT * math.float3(0f, 1f, 0f)
                )
                .Schedule(inputDependencies);
    }
}
