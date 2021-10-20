using Integration;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(FixedUpdateGroup))]
[UpdateBefore(typeof(ForceCalculationSystems))]
public class ClearForceSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .ForEach(
                (ref Force force) => force.Value = new Unity.Mathematics.float3(0f, 0f, 0f)
            )
            .Schedule();
    }
}