using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateBefore(typeof(ForceCalculationSystems))]
public class ClearForceSystem : JobComponentSystem
{
    [BurstCompile]
    struct ClearForceJob : IJobForEach<Force>
    {   
        public void Execute(
            ref Force force)
        {
            force.Value = new Unity.Mathematics.float3(0f,0f,0f);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return new ClearForceJob().Schedule(this, inputDependencies);
    }
}