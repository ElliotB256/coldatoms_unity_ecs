using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// Applies gravitational force to all entities with mass.
/// </summary>
[UpdateInGroup(typeof(ForceCalculationSystems))]
public class GravitySystem : JobComponentSystem
{
    public const float GRAVITATIONAL_CONSTANT = 2.0f;

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        return new ApplyGravityJob { g = GRAVITATIONAL_CONSTANT * math.float3(0f, -1f, 0f) }.Schedule(this, inputDependencies);
    }

    [BurstCompile]
    struct ApplyGravityJob : IJobForEach<Mass, Force>
    {
        public float3 g;

        public void Execute(
            [ReadOnly] ref Mass mass, ref Force force)
        {
            force.Value = force.Value + mass.Value * g;
        }
    }
}
