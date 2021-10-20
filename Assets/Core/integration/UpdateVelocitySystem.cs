using Integration;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

/// <summary>
/// Update velocity according to velocity verlet.
/// </summary>
[UpdateAfter(typeof(ForceCalculationSystems))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class UpdateVelocitySystem : SystemBase
{
    protected override void OnUpdate()
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        Entities.ForEach(
            (
                ref Velocity velocity,
                in Mass mass, 
                in Force force, 
                in PrevForce oldForce
                ) =>
                 velocity.Value = velocity.Value + 0.5f * (force.Value + oldForce.Value) * DeltaTime / mass.Value
            ).Schedule();
    }
}