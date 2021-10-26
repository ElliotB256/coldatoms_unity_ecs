﻿using Integration;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// Deletes any atoms that leave the simulation bounds.
/// </summary>
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class OutOfBoundsSystem : JobComponentSystem
{
    public const float OUT_OF_BOUNDS_LIMIT = 150.0f;
    public const float VAC_RADIUS = 15f;

    EntityCommandBufferSystem CommandBufferSystem;

    protected override void OnCreate()
    {
        CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = CommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        
        var jobHandle = Entities
            .WithAll<Atom>()
            .ForEach(
            (Entity atom, int entityInQueryIndex, ref Translation translation) =>
                {
                    var twoD = translation.Value;
                    twoD.y = 0f;

                    if (math.lengthsq(translation.Value) > OUT_OF_BOUNDS_LIMIT * OUT_OF_BOUNDS_LIMIT)
                        commandBuffer.DestroyEntity(entityInQueryIndex, atom);
                    else if (math.lengthsq(twoD) > VAC_RADIUS * VAC_RADIUS)
                        commandBuffer.DestroyEntity(entityInQueryIndex, atom);
                }
            )
            .Schedule(inputDeps);

        CommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}
