using Integration;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(FixedUpdateGroup))]
[UpdateBefore(typeof(ForceCalculationSystems))]
public class ClearForceSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return Entities
            .ForEach(
                (ref Force force) => force.Value = new Unity.Mathematics.float3(0f, 0f, 0f)
            )
            .Schedule(inputDependencies);
    }
}