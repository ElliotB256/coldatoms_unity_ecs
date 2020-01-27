using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Calculation
{
    /// <summary>
    /// A per-entity quantity which can be aggregated.
    /// </summary>
    public interface IAggregatable
    {
        float GetValue();
    }

    public struct Total : IComponentData { }
    public struct Average : IComponentData { }
    public struct Min : IComponentData { }
    public struct Max : IComponentData { }

    [UpdateInGroup(typeof(CalculationSystemGroup))]
    public class AggregateQuantitiesSystem<TAQuantity, TMarker> : JobComponentSystem
        where TAQuantity : struct, IComponentData, IAggregatable
        where TMarker : struct, IComponentData
    {
        protected override void OnCreateManager()
        {
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var totalPerGroup = new NativeArray<float>(Group.NUMBER_OF_GROUPS, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            var minPerGroup = new NativeArray<float>(Group.NUMBER_OF_GROUPS, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            var maxPerGroup = new NativeArray<float>(Group.NUMBER_OF_GROUPS, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            var numberPerGroup = new NativeArray<int>(Group.NUMBER_OF_GROUPS, Allocator.TempJob, NativeArrayOptions.ClearMemory);

            // Aggregate quantities
            var aggregate = new AggregateJob
            {
                TotalPerGroup = totalPerGroup,
                MinPerGroup = minPerGroup,
                MaxPerGroup = maxPerGroup,
                NumberPerGroup = numberPerGroup
            }
            .ScheduleSingle(this, inputDeps);

            var updateMax = new UpdateCurrentValueJob<Max>()
            {
                Values = maxPerGroup,
                Scale = 1.0f
            }
            .Schedule(this, aggregate);

            var updateMin = new UpdateCurrentValueJob<Min>()
            {
                Values = minPerGroup,
                Scale = 1.0f
            }
            .Schedule(this, updateMax);

            var updateTotal = new UpdateCurrentValueJob<Total>()
            {
                Values = totalPerGroup,
                Scale = 1.0f
            }
            .Schedule(this, updateMin);

            var updateAverage = new WeightedUpdateCurrentValueJob<Average>()
            {
                Values = totalPerGroup,
                Numbers = numberPerGroup
            }
            .Schedule(this, updateTotal);

            minPerGroup.Dispose(updateMin);
            maxPerGroup.Dispose(updateMax);
            totalPerGroup.Dispose(updateAverage);
            numberPerGroup.Dispose(updateAverage);

            return updateAverage;

        }

        [BurstCompile]
        [RequireComponentTag(typeof(Atom),typeof(Trapped))]
        struct AggregateJob : IJobForEach<TAQuantity>
        {
            public NativeArray<float> TotalPerGroup;
            public NativeArray<float> MinPerGroup;
            public NativeArray<float> MaxPerGroup;
            public NativeArray<int> NumberPerGroup;

            public void Execute(ref TAQuantity quantity)
            {
                TotalPerGroup[0] = TotalPerGroup[0] + quantity.GetValue();
                MinPerGroup[0] = math.min(MinPerGroup[0], quantity.GetValue());
                MaxPerGroup[0] = math.max(MaxPerGroup[0], quantity.GetValue());
                NumberPerGroup[0] = NumberPerGroup[0] + 1;
            }
        }

        [BurstCompile]
        struct UpdateCurrentValueJob<TStrategy> : IJobForEach<CurrentDataValue, TMarker, TStrategy>
            where TStrategy : struct, IComponentData
        {
            public NativeArray<float> Values;
            public float Scale;

            public void Execute(ref CurrentDataValue current, [ReadOnly] ref TMarker marker, [ReadOnly] ref TStrategy strategy)
            {
                current.Value = Values[0] / Scale;
            }
        }

        [BurstCompile]
        struct WeightedUpdateCurrentValueJob<TStrategy> : IJobForEach<CurrentDataValue, TMarker, TStrategy>
            where TStrategy : struct, IComponentData
        {
            public NativeArray<float> Values;
            public NativeArray<int> Numbers;

            public void Execute(ref CurrentDataValue current, [ReadOnly] ref TMarker marker, [ReadOnly] ref TStrategy strategy)
            {
                current.Value = Values[0] / Numbers[0];
            }
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
    public class CalculateAggregateTotalEnergySystem : AggregateQuantitiesSystem<TotalEnergy, TotalEnergyData> { }
    public struct TotalEnergyData : IComponentData { }
}