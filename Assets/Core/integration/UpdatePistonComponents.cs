using Integration;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

/// <summary>
/// Update Piston translation value from UpdatePistonPositionSystem
    // Im sure I am going about this wrong and don't need two systems for this
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class UpdatePistonComponentsSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
        // Enabled = true;
    }
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return Entities
            .WithNone<PrevForce>()
            .ForEach(
            (ref Translation translation, in Piston piston) => {
                translation.Value = piston.Translation;
            }).Schedule(inputDependencies);
    }
}
