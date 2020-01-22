using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

/// <summary>
/// Update position according to velocity verlet.
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
public class UpdatePositionSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = Time.fixedDeltaTime;
        return Entities.ForEach(
            (
                ref Translation translation,
                in Mass mass,
                in PrevForce force,
                in Velocity velocity
                ) =>
                translation.Value = translation.Value + velocity.Value * DeltaTime + 0.5f * (force.Value / mass.Value) * DeltaTime * DeltaTime
            ).Schedule(inputDependencies);
    }
}