using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

/// <summary>
/// Updates the size of the RF knife entity so that the visual representation matches the knife radius.
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
public class UpdateKnifeScaleSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(RFKnife))]
    struct UpdateKnifeSizeJob : IJobForEach<Radius,Scale>
    {
        public void Execute(
            [ReadOnly] ref Radius radius,
            ref Scale scale)
        {
            scale.Value = radius.Value * 2f;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return new UpdateKnifeSizeJob().Schedule(this, inputDependencies);
    }
}