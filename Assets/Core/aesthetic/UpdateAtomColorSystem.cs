using Unity.Entities;
using Unity.Jobs;

[
    UpdateAfter(typeof(IncreaseCollisionTimerSystem)),
]
public class UpdateAtomColorSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach(
            (ref ShaderCollisionTime time, in CollisionStats stats) =>
                time.Value = stats.TimeSinceLastCollision
            ).ScheduleParallel();
    }
}