using Integration;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

/// <summary>
/// Update Piston positions
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class UpdatePistonPositionSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
        // Enabled = true;
    }
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        return Entities
            // .WithName("Piston")
            .WithAll<Piston>()
            .ForEach(
            (ref Translation translation, ref Velocity velocity) => {
                translation.Value += velocity.Value * DeltaTime;

                    // Bouncing the Piston back and forth to prevent 0 volume conditions
                    // Maybe have this in another system
                if (translation.Value.x < -5 || translation.Value.x > 5)
                {
                    velocity.Value *= -1;
                }
            }).Schedule(inputDependencies);
    }
}


