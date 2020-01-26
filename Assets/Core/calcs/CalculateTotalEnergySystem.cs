using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Calculation
{
    [
        UpdateInGroup(typeof(CalculationSystemGroup)),
        UpdateAfter(typeof(CalculateKineticEnergySystem))
        ]
    public class CalculateTotalEnergySystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return Entities.ForEach(
                (ref TotalEnergy energy, in KineticEnergy ke, in PotentialEnergy pe) =>
                    energy.Value = ke.Value + pe.Value
                )
                .Schedule(inputDeps);
        }
    }
}