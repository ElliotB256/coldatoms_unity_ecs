using Integration;
using Unity.Entities;
using Unity.Jobs;

[UpdateBefore(typeof(ForceCalculationSystems))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class ClearPotentialEnergySystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .ForEach(
                (ref PotentialEnergy energy) => energy.Value = 0f
            )
            .Schedule();
    }
}