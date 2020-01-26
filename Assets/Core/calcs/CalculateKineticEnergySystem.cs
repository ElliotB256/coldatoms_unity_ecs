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

    [
        UpdateInGroup(typeof(CalculationSystemGroup)),
        UpdateAfter(typeof(CalculateAtomKineticEnergySystem)),
        AlwaysUpdateSystem
        ]
    public class CalculateAggregateKineticEnergySystem : AggregateQuantitiesSystem<KineticEnergy, KineticEnergyData> { }
    public struct KineticEnergyData : IComponentData { }

    [
        UpdateInGroup(typeof(CalculationSystemGroup)),
        UpdateAfter(typeof(CalculateAggregateKineticEnergySystem)),
        AlwaysUpdateSystem
        ]
    public class CalculateAggregatePotentialEnergySystem : AggregateQuantitiesSystem<PotentialEnergy, PotentialEnergyData> { }
    public struct PotentialEnergyData : IComponentData { }

    [
        UpdateInGroup(typeof(CalculationSystemGroup)),
        UpdateAfter(typeof(CalculateAggregatePotentialEnergySystem)),
        AlwaysUpdateSystem
        ]
    public class CalculateAggregateTotalEnergySystem : AggregateQuantitiesSystem<PotentialEnergy, TotalEnergyData> { }
    public struct TotalEnergyData : IComponentData { }
}