using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateAfter(typeof(UpdateVelocitySystem))]
public class SavePrevForceSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return Entities.ForEach(
            (ref PrevForce prevForce, in Force force) =>
                prevForce.Value = force.Value
                ).Schedule(inputDependencies);
    }
}