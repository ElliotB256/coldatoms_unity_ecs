using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateBefore(typeof(ForceCalculationSystems))]
public class IncreaseCollisionTimerSystem : JobComponentSystem
{ 
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = Time.DeltaTime;
        return Entities.ForEach(
            (ref CollisionStats stat) =>
                stat.TimeSinceLastCollision = stat.TimeSinceLastCollision + DeltaTime
            ).Schedule(inputDependencies);
    }
}