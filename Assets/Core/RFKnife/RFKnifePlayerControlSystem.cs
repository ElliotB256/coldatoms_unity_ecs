using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Updates the size of the RF knife entity so that the visual representation matches the knife radius.
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
public class RFKnifePlayerControlSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float dT = Time.DeltaTime;
        Entities
            .WithAll<RFKnife>()
            .ForEach(
                (ref Radius radius, in PlayerInputs controls) =>
                {
                    radius.Value *= (1 + controls.VerticalAxis * dT / 2);
                }
            ).Schedule();
    }
}