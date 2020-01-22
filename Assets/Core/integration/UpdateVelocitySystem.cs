using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

/// <summary>
/// Update velocity according to velocity verlet.
/// </summary>
[UpdateAfter(typeof(ForceCalculationSystems))]
public class UpdateVelocitySystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = Time.fixedDeltaTime;
        return Entities.ForEach(
            (
                ref Velocity velocity,
                in Mass mass, 
                in Force force, 
                in PrevForce oldForce
                ) =>
                 velocity.Value = velocity.Value + 0.5f * (force.Value + oldForce.Value) * DeltaTime / mass.Value
            ).Schedule(inputDependencies);
    }
}