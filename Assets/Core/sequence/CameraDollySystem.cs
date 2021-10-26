using Unity.Entities;
using Unity.Transforms;

public class CameraDollySystem : SystemBase
{
    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();

        if (sequence.Stage != SequenceStage.PullCameraBack && sequence.Stage != SequenceStage.RestoreCamera)
            return;

        var dolly = GetSingleton<CameraDolly>();

        //Prevent atoms from colliding, launch them.
        Dependency = Entities
            .WithAll<CameraDolly>()
            .ForEach(
            (Entity e, ref Translation transform) =>
                {
                    transform.Value = dolly.GetPosition(sequence.Stage);
                }
        )
        .WithoutBurst()
        .Schedule(Dependency);

        dolly.Elapsed += Time.DeltaTime;

        if (dolly.Elapsed > dolly.Duration)
        {
            sequence.Stage++;
            dolly.Elapsed = 0f;
        }

        SetSingleton(sequence);
        SetSingleton(dolly);
    }
}
