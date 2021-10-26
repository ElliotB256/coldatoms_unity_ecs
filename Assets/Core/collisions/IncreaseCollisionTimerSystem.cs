using Integration;
using Unity.Entities;
using Unity.Jobs;

[UpdateBefore(typeof(ForceCalculationSystems))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class IncreaseCollisionTimerSystem : SystemBase
{ 
    protected override void OnUpdate()
    {
        float DeltaTime = FixedUpdateGroup.FixedTimeDelta;
        Entities.ForEach(
            (ref CollisionStats stat) =>
                stat.TimeSinceLastCollision = stat.TimeSinceLastCollision + DeltaTime
            ).ScheduleParallel();
    }
}