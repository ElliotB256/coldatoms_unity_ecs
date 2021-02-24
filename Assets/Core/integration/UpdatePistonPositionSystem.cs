﻿using Integration;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

/// <summary>
/// Update Piston positions
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class UpdatePistonPositionSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
        // Enabled = true;
    }
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        return Entities
            .WithName("Piston")
            .ForEach(
            (ref Piston piston) => {
                piston.Translation += piston.Velocity * DeltaTime;
                // if (piston.Translation.x < -3 || piston.Translation.x > 3)
                // {
                //     piston.Velocity *= -1;
                // }
            }).Schedule(inputDependencies);
    }
}


