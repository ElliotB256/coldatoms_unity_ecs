using Integration;
using Unity.Entities;

[UpdateInGroup(typeof(FixedUpdateGroup))]
public class WaitAfterReadoutSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();

        if (sequence.Stage != SequenceStage.WaitAfterReadout)
            return;

        float delay = 1f;
        sequence.Elapsed += FixedUpdateGroup.FixedTimeDelta;
        if (sequence.Elapsed > delay)
        {
            sequence.Elapsed = 0f;
            sequence.Stage++;
        }

        SetSingleton(sequence);
    }
}
