using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Sequence : IComponentData
{
    public SequenceStage Stage;
    public float LaunchSpeed;
    public float Elapsed;
    public float BallisticDuration;
    public float BallisticReadoutDuration;
    public float ReadoutDelay;
    public float PhotonRecoilVelocity;
    public int SignalPeriod;
    public int SignalCurrent;
    public float StartingTime;
    public Entity GraphPointTemplate;
    public Entity GraphCentrePointTemplate;
    public float BeamRadius;
    public float VelocitySelectionWidth;
    public float MeasurementRegionWidth;

    public float GetStartingPhase() => math.PI * 2.0f * SignalCurrent / SignalPeriod;
}

public enum SequenceStage : byte
{
    Initialisation,
    TurnOnKnife,
    Evaporation,
    TurnOffKnife,
    PullCameraBack,
    Launch,
    CreateSuperposition,
    Ballistic1,
    Mirror,
    Ballistic2,
    FinalBeamSplitter,
    Ballistic3,
    FreezeForReadout,
    MakeMeasurement,
    WaitAfterReadout,
    RestoreCamera,
    Final
}