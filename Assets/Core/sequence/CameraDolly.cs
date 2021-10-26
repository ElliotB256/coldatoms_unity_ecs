using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct CameraDolly : IComponentData
{
    public float3 FinalPosition;
    public float3 InitialPosition;
    public float Duration;
    public float Elapsed;

    public float3 GetPosition(SequenceStage stage)
    {
        var a = stage == SequenceStage.PullCameraBack ? InitialPosition : FinalPosition;
        var b = stage == SequenceStage.PullCameraBack ? FinalPosition : InitialPosition;
        return new float3(
        Mathf.SmoothStep(a.x, b.x, math.clamp(Elapsed / Duration, 0f, 1f)),
        Mathf.SmoothStep(a.y, b.y, math.clamp(Elapsed / Duration, 0f, 1f)),
        Mathf.SmoothStep(a.z, b.z, math.clamp(Elapsed / Duration, 0f, 1f))
        );
    }
}
