using Integration;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
// using Unity.Transforms;

/// <summary>
/// Update Piston positions
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class UpdatePistonPositionSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
        Enabled = false;
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        return Entities.ForEach(
            (
                ref Velocity velocity,
                in Position position
                ) =>
                position.Value = position.Value + velocity.Value * DeltaTime
            ).Schedule(inputDependencies);
    }
}
