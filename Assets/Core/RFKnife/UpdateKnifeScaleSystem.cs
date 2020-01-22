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
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return Entities.WithAll<RFKnife>().ForEach(
            (ref Scale scale, in Radius radius) =>
                scale.Value = radius.Value * 2f
            ).Schedule(inputDependencies);
    }
}