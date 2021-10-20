using Unity.Entities;
using Unity.Transforms;

/// <summary>
/// Updates the size of the RF knife entity so that the visual representation matches the knife radius.
/// </summary>
public class UpdateKnifeScaleSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<RFKnife>().ForEach(
            (ref Scale scale, in Radius radius) =>
                scale.Value = radius.Value * 2f
            ).Schedule();
    }
}