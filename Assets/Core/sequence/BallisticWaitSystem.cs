using Integration;
using Unity.Entities;

[UpdateInGroup(typeof(FixedUpdateGroup))]
public class BallisticWaitSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();

        if (sequence.Stage != SequenceStage.Ballistic1 && sequence.Stage != SequenceStage.Ballistic2 && sequence.Stage != SequenceStage.Ballistic3)
            return;

        float delay = 0f;
        switch (sequence.Stage)
        {
            case SequenceStage.Ballistic1:
            case SequenceStage.Ballistic2:
                delay = sequence.BallisticDuration;
                break;
            case SequenceStage.Ballistic3:
                delay = sequence.BallisticReadoutDuration;
                break;
        }

        sequence.Elapsed += FixedUpdateGroup.FixedTimeDelta;
        if (sequence.Elapsed > delay)
        {
            sequence.Elapsed = 0f;
            sequence.Stage++;
        }

        SetSingleton(sequence);
    }
}
