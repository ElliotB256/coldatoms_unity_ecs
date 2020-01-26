using Unity.Entities;
using Unity.Jobs;

namespace Calculation
{
    /// <summary>
    /// Scrolls the X-axis limits of a time-series chart
    /// </summary>
    [UpdateInGroup(typeof(PostCalculationGroup))]
    public class UpdateChartXAxisLimitsSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float now = (float)Time.ElapsedTime;
            return Entities
                .WithAll<TimeData>()
                .ForEach(
                    (ref AxisLimit limit) =>
                    {
                        float range = limit.Max - limit.Min;
                        limit.Max = now;
                        limit.Min = now - range;
                    }
                )
                .Schedule(inputDeps);
        }
    }
}