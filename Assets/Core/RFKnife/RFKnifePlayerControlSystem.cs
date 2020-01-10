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
public class RFKnifePlayerControlSystem : JobComponentSystem
{
    [RequireComponentTag(typeof(RFKnife))]
    struct PlayerControlRFKnifeJob : IJobForEach<PlayerInputs,Radius>
    {
        public float dT;

        public void Execute(
            [ReadOnly] ref PlayerInputs controls,
            ref Radius radius)
        {
            radius.Value *= (1 + controls.VerticalAxis * dT / 2);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return new PlayerControlRFKnifeJob { dT = Time.DeltaTime }.Schedule(this, inputDependencies);
    }
}