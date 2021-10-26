using Unity.Entities;
using Unity.Mathematics;

public class MirrorSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();

        if (sequence.Stage != SequenceStage.Mirror)
            return;

        var flipVelocity = new float3(0f, sequence.PhotonRecoilVelocity, 0f);

        Entities.WithAll<Atom, Upper>().ForEach((ref Velocity vel) => vel.Value -= flipVelocity).Schedule();
        Entities.WithNone<Upper>().WithAll<Atom>().ForEach((ref Velocity vel) => vel.Value += flipVelocity).Schedule();

        sequence.Stage++;
        SetSingleton(sequence);
    }
}
