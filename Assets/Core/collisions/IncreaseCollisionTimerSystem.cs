using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[UpdateBefore(typeof(ForceCalculationSystems))]
public class IncreaseCollisionTimerSystem : JobComponentSystem
{
    [BurstCompile]
    struct IncreaseCollisionTimerJob : IJobForEach<CollisionStats>
    {
        public float DeltaTime;

        public void Execute(
            ref CollisionStats stat)
        {
            stat.TimeSinceLastCollision += DeltaTime;
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return new IncreaseCollisionTimerJob { DeltaTime = Time.deltaTime }.Schedule(this, inputDependencies);
    }
}