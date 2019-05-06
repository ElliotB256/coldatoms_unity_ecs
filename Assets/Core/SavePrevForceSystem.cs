using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateAfter(typeof(UpdateVelocitySystem))]
public class SavePrevForceSystem : JobComponentSystem
{
    [BurstCompile]
    struct SavePrevForceJob : IJobForEach<Force, PrevForce>
    {   
        public void Execute(
            ref Force force, [ReadOnly] ref PrevForce oldForce)
        {
            oldForce.Value = force.Value;
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return new SavePrevForceJob().Schedule(this, inputDependencies);
    }
}