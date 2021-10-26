using Integration;
using Unity.Entities;
using Unity.Jobs;

namespace Calculation
{
    [UpdateInGroup(typeof(PostCalculationGroup))]
    public class SampleDataSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float deltaTime = FixedUpdateGroup.FixedTimeDelta;
            return Entities
                .ForEach(
                    (DynamicBuffer<DataPoint> data, ref SamplingInterval interval, in CurrentDataValue current) =>
                    {
                        interval.Remaining = interval.Remaining - deltaTime;
                        if (interval.Remaining < 0f)
                        {
                            interval.Remaining = interval.Interval;
                            data.Add(new DataPoint { Value = current.Value });
                        }
                    }
                )
                .Schedule(inputDeps);
        }
    }
}