using Unity.Entities;
using Unity.Jobs;

public class UpdateTimeAliveSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var dt = Time.DeltaTime;
        Entities.ForEach(
            (ref TimeAlive time) =>
                time.Value += dt
            ).ScheduleParallel();
    }
}