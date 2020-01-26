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
        private EntityQuery AtomQuery;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();
            Enabled = false;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var totalPerGroup = new NativeArray<float>(Group.NUMBER_OF_GROUPS, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            var minPerGroup = new NativeArray<float>(Group.NUMBER_OF_GROUPS, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            var maxPerGroup = new NativeArray<float>(Group.NUMBER_OF_GROUPS, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            int atomNumber = AtomQuery.CalculateEntityCount();

            // Aggregate quantities
            var aggregate = Entities
                .ForEach(
                (in TAQuantity q) =>
                {
                    totalPerGroup[0] = totalPerGroup[0] + q.GetValue();
                    minPerGroup[0] = math.min(minPerGroup[0], q.GetValue());
                    maxPerGroup[0] = math.max(maxPerGroup[0], q.GetValue());
                }
                )
                .Schedule(inputDeps);

            var updateMax = Entities
                .WithAll<TMarker, Max>()
                .ForEach(
                (ref CurrentDataValue current) => current.Value = maxPerGroup[0]
                )
                .Schedule(aggregate);

            var updateMin = Entities
                .WithAll<TMarker, Min>()
                .ForEach(
                (ref CurrentDataValue current) => current.Value = minPerGroup[0]
                )
                .Schedule(updateMax);

            var updateTotal = Entities
                .WithAll<TMarker, Total>()
                .ForEach(
                (ref CurrentDataValue current) => current.Value = totalPerGroup[0]
                )
                .Schedule(updateMin);

            var updateMean = Entities
                .WithAll<TMarker, Total>()
                .ForEach(
                (ref CurrentDataValue current) => current.Value = totalPerGroup[0] / atomNumber
                )
                .Schedule(updateTotal);

            totalPerGroup.Dispose(updateTotal);
            minPerGroup.Dispose(updateMin);
            maxPerGroup.Dispose(updateMax);

            return updateMean;

        }

        struct UpdateCurrentValueJob<TStrategy> : IJobForEach<CurrentDataValue, TMarker, TStrategy>
            where TStrategy : struct, IComponentData
        {
            public NativeArray<float> Values;

            public void Execute(ref CurrentDataValue current, [ReadOnly] ref TMarker marker, [ReadOnly] ref TStrategy strategy)
            {
                current.Value = Values[0];
            }
        }
    }
}