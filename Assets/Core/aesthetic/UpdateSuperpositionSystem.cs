using Unity.Entities;
using Unity.Jobs;

[
    UpdateAfter(typeof(IncreaseCollisionTimerSystem)),
]
public class UpdateSuperpositionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();
        bool passed = sequence.Stage < SequenceStage.Mirror;
        passed = false;

        Entities.WithNone<Upper>().ForEach(
            (ref Superposition superpos) =>
                superpos.Value = passed ? 1f : 0f
            ).ScheduleParallel();

        Entities.WithAll<Upper>().ForEach(
            (ref Superposition superpos) =>
                superpos.Value = passed ? 0f : 1f
            ).ScheduleParallel();
    }
}