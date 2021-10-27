using Unity.Entities;
using Unity.Mathematics;

[
    UpdateAfter(typeof(IncreaseCollisionTimerSystem)),
]
public class ChangeBeamPowerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float dt = Time.DeltaTime;
        Entities.ForEach(
            (ref BeamPower power) =>
                power.Value = math.clamp(power.Value -= dt, 0f, 1f)
            ).Schedule();
    }
}