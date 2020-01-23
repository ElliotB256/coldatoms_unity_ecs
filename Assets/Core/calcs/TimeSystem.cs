using Unity.Entities;
using Unity.Jobs;

namespace Calculation
{
    [UpdateInGroup(typeof(CalculationSystemGroup))]
    public class TimeSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float time = (float)Time.ElapsedTime;
            return Entities
                .WithAll<TimeData>()
                .ForEach((ref CurrentDataValue data) => data.Value = time)
                .Schedule(inputDeps);
        }
    }

    public struct TimeData : IComponentData { }
}