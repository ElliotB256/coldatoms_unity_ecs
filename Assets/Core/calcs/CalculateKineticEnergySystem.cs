using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Calculation
{
    [UpdateInGroup(typeof(CalculationSystemGroup))]
    public class CalculateAtomKineticEnergySystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return Entities.ForEach(
                (ref KineticEnergy ke, in Mass mass, in Velocity velocity) =>
                    ke.Value = mass.Value * math.lengthsq(velocity.Value) / 2.0f
                )
                .Schedule(inputDeps);
        }
    }
}