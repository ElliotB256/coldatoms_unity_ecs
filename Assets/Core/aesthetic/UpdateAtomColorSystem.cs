using Unity.Entities;
using Unity.Jobs;

[
    UpdateBefore(typeof(ForceCalculationSystems)),
    UpdateAfter(typeof(IncreaseCollisionTimerSystem)),
    AlwaysUpdateSystem
]
public class UpdateAtomColorSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach(
            (ref ShaderCollisionTime time, in CollisionStats stats) =>
                time.Value = stats.TimeSinceLastCollision
            ).Schedule();
    }
}