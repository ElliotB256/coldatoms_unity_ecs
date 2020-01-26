using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateBefore(typeof(ForceCalculationSystems))]
public class ClearPotentialEnergySystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return Entities
            .ForEach(
                (ref PotentialEnergy energy) => energy.Value = 0f
            )
            .Schedule(inputDependencies);
    }
}