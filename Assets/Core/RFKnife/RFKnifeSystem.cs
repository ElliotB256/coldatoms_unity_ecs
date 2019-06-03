using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// Untrap atoms that are outside of RF knife sphere.
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
public class RFKnifeSystem : JobComponentSystem
{
    public float KnifeRadius = 30.0f;
    public float3 KnifePosition = new float3(0, 1, 0);

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return new UntrapJob {radius = KnifeRadius, knifePosition = KnifePosition}.Schedule(this, inputDependencies);
    }

    [BurstCompile]
    struct UntrapJob : IJobForEach<Translation, Trapped>
    {
        public float radius;
        public float3 knifePosition;

        public void Execute(
            [ReadOnly] ref Translation position, ref Trapped trapped)
        {
            // Assuming RF Knife is located at (0,1,0) for now
            if (math.lengthsq(position.Value - knifePosition) > radius) { trapped.Value = new Boolean(false); }
        }
    }

}
