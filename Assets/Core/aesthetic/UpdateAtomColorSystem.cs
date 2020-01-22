using Unity.Entities;
using Unity.Jobs;

[
    UpdateBefore(typeof(ForceCalculationSystems)),
    UpdateAfter(typeof(IncreaseCollisionTimerSystem)),
    AlwaysUpdateSystem
]
public class UpdateAtomColorSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return Entities.ForEach(
            (ref ShaderCollisionTime time, in CollisionStats stats) =>
                time.Value = stats.TimeSinceLastCollision
            ).Schedule(inputDependencies);
    }
}