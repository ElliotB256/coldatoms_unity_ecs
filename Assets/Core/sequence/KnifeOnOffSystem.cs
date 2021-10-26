using Unity.Entities;

public class KnifeOnOffSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();

        if (sequence.Stage != SequenceStage.TurnOffKnife && sequence.Stage != SequenceStage.TurnOnKnife)
            return;

        Dependency = Entities
            .ForEach(
            (ref KnifeVisibility vis) =>
                {
                    vis.Value = sequence.Stage == SequenceStage.TurnOnKnife ? 1.0f : 0.0f;
                }
        )
        .Schedule(Dependency);

        Dependency = Entities.ForEach((ref TabularRFKnife table) => table.CurrentTime = 0f).Schedule(Dependency);
        Dependency = Entities.WithAll<RFKnife>().ForEach((ref Radius radius) => radius.Value = 10f).Schedule(Dependency);

        sequence.Stage++;
        SetSingleton(sequence);
    }
}
