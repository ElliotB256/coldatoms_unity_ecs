using Unity.Entities;
using Unity.Jobs;

namespace Calculation
{
    public class CapBufferLengthSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return Entities.ForEach(
                (ref DynamicBuffer<DataPoint> buffer, ref DataLength length) =>
                {
                    var delta = buffer.Length - length.Value;
                    if (delta > 0)
                        buffer.RemoveRange(0, delta);
                })
                .Schedule(inputDeps);
        }
    }
}
