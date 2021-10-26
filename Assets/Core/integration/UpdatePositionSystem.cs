using Integration;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

/// <summary>
/// Update position according to velocity verlet.
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class UpdatePositionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float DeltaTime = FixedUpdateGroup.FixedTimeDelta;
        Entities.ForEach(
            (
                ref Translation translation,
                in Mass mass,
                in PrevForce force,
                in Velocity velocity
                ) =>
                translation.Value = translation.Value + velocity.Value * DeltaTime + 0.5f * (force.Value / mass.Value) * DeltaTime * DeltaTime
            ).Schedule();
    }
}