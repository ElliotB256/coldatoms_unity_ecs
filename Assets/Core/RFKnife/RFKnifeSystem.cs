using Integration;
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
public class RFKnifeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (!GetRFKnifeSystem.KnifeExists)
            return;

        float rSq = Mathf.Pow(GetRFKnifeSystem.Radius, 2f);
        var knifePosition = GetRFKnifeSystem.Position;
        var buffer = CommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        Dependency = Entities
            .WithAll<Trapped>()
            .ForEach(
                (Entity e, int entityInQueryIndex, in Translation t) =>
                {
                    if (math.lengthsq(t.Value - knifePosition) > rSq)
                        buffer.RemoveComponent<Trapped>(entityInQueryIndex, e);
                }
            ).Schedule(Dependency);

        CommandBufferSystem.AddJobHandleForProducer(Dependency);
    }

    private RFKnifeCommandBufferSystem CommandBufferSystem;
    private GetRFKnifeSystem GetRFKnifeSystem;

    protected override void OnStartRunning()
    {
        CommandBufferSystem = World.GetOrCreateSystem<RFKnifeCommandBufferSystem>();
        GetRFKnifeSystem = World.GetOrCreateSystem<GetRFKnifeSystem>();
    }
}
