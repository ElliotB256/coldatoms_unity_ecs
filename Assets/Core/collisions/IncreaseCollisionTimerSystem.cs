using Integration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateBefore(typeof(ForceCalculationSystems))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class IncreaseCollisionTimerSystem : JobComponentSystem
{ 
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        return Entities.ForEach(
            (ref CollisionStats stat) =>
                stat.TimeSinceLastCollision = stat.TimeSinceLastCollision + DeltaTime
            ).Schedule(inputDependencies);
    }
}