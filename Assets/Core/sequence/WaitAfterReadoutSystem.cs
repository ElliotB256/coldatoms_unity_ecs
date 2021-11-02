using Integration;
using Unity.Entities;

[UpdateInGroup(typeof(FixedUpdateGroup))]
public class WaitAfterReadoutSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();
        var input = GetSingleton<PlayerInputs>();

        if (sequence.Stage != SequenceStage.WaitAfterReadout)
            return;

        float delay = 0.5f;
        sequence.Elapsed += FixedUpdateGroup.FixedTimeDelta;
        if (sequence.Elapsed > delay)
        {
            sequence.Elapsed = 0f;
            if (input.advance)
            {
                sequence.Stage++;
                input.advance = false;
            }
        }

        SetSingleton(sequence);
        SetSingleton(input);
    }
}
