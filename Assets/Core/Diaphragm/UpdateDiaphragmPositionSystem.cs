using Integration;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

/// <summary>
/// Update Diaphragm positions
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class UpdateDiaphragmPositionSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
        // Enabled = true;
    }
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        return Entities
            .WithAll<Diaphragm>()
            .ForEach(
            (ref Translation translation, ref Velocity velocity) => {
                translation.Value += velocity.Value * DeltaTime;
                
                    // Bounce the diaphram off the x+ wall
                        // Might need to also bounce of the piston but hopefully there will always be particles inbetween.
                if (translation.Value.x > 10f)
                {
                    velocity.Value.x *= -1f;
                }
            }).Schedule(inputDependencies);
    }
}


