using Unity.Entities;
using UnityEngine;

public class SequenceControlSystem : SystemBase
{

    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();
        var controls = GetSingleton<PlayerInputs>();

        if (controls.doubleClicked && sequence.Stage == SequenceStage.Evaporation)
        {
            sequence.Stage++;
        }

        if (sequence.Stage == SequenceStage.Final)
        {
            sequence.Stage = SequenceStage.Initialisation;
            sequence.SignalCurrent = sequence.SignalCurrent + 1 % sequence.SignalPeriod;
        }

#if UNITY_EDITOR

#else
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
#endif
        SetSingleton(sequence);
    }
}
