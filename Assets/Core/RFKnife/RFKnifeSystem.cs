using Integration;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Untrap atoms that are outside of RF knife sphere.
/// </summary>
[UpdateInGroup(typeof(FixedUpdateGroup))]
[UpdateBefore(typeof(ForceCalculationSystems))]
public class RFKnifeSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        if (!GetRFKnifeSystem.KnifeExists)
            return inputDependencies;
        var jobHandle = new UntrapJob
        {
            rSq = Mathf.Pow(GetRFKnifeSystem.Radius, 2f),
            knifePosition = GetRFKnifeSystem.Position,
            Buffer = CommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDependencies);
        CommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }

    private RFKnifeCommandBufferSystem CommandBufferSystem;
    private GetRFKnifeSystem GetRFKnifeSystem;

    protected override void OnStartRunning()
    {
        CommandBufferSystem = World.GetOrCreateSystem<RFKnifeCommandBufferSystem>();
        GetRFKnifeSystem = World.GetOrCreateSystem<GetRFKnifeSystem>();
    }

    [RequireComponentTag(typeof(Trapped))]
    struct UntrapJob : IJobForEachWithEntity<Translation>
    {
        public float rSq;
        public float3 knifePosition;
        public EntityCommandBuffer.Concurrent Buffer;

        public void Execute(
            Entity e, int i,
            [ReadOnly] ref Translation position)
        {
            if (math.lengthsq(position.Value - knifePosition) > rSq)
                Buffer.RemoveComponent<Trapped>(i, e);
        }
    }

}
