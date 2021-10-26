using Integration;
using Unity.Entities;

[UpdateInGroup(typeof(FixedUpdateGroup))]
[UpdateAfter(typeof(ForceCalculationSystems))]
[UpdateBefore(typeof(UpdateVelocitySystem))]
public class FreezeForReadoutSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();

        if (sequence.Stage != SequenceStage.FreezeForReadout)
            return;

        Entities.WithAll<Atom>().ForEach((ref Velocity vel, ref Force force) => {
            vel.Value = new Unity.Mathematics.float3();
            force.Value = new Unity.Mathematics.float3();
            }
            ).Schedule();

        sequence.Elapsed += FixedUpdateGroup.FixedTimeDelta;
        if (sequence.Elapsed > sequence.ReadoutDelay)
        {
            sequence.Elapsed = 0f;
            sequence.Stage++;
        }

        SetSingleton(sequence);
    }
}
