using Integration;
using Unity.Entities;

[UpdateInGroup(typeof(FixedUpdateGroup))]
public class InitialiseSequencesystem : SystemBase
{
    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();

        if (sequence.Stage != SequenceStage.Initialisation)
            return;

        Entities.ForEach((ref AtomCloud cloud) => cloud.ShouldSpawn = true).Schedule();
        
        sequence.Stage++;
        SetSingleton(sequence);
    }
}
