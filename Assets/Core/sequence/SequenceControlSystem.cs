using Unity.Entities;
using UnityEngine;

public class SequenceControlSystem : SystemBase
{

    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();

        if (Input.GetKeyDown(KeyCode.KeypadEnter) && sequence.Stage == SequenceStage.Evaporation)
        {
            sequence.Stage++;
        }

        if (sequence.Stage == SequenceStage.Final)
        {
            sequence.Stage = SequenceStage.Initialisation;
            sequence.SignalCurrent = sequence.SignalCurrent + 1 % sequence.SignalPeriod;
        }

        SetSingleton(sequence);
    }
}
