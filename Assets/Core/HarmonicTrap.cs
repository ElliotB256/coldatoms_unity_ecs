using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[UpdateInGroup(typeof(ForceCalculationSystems))]
public class HarmonicTrap : JobComponentSystem
{
    [BurstCompile]
    struct HarmonicTrapJob : IJobForEach<Translation, Force>
    {   
        public void Execute(
            ref Translation translation, [ReadOnly] ref Force force)
        {
            //force.Value = force.Value - translation.Value;
            force.Value = new Unity.Mathematics.float3(0f, 0f, -translation.Value.z);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return new HarmonicTrapJob().Schedule(this, inputDependencies);
    }
}
