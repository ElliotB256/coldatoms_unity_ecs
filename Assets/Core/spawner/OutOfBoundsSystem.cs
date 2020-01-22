using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

/// <summary>
/// Deletes any atoms that leave the simulation bounds.
/// </summary>
public class OutOfBoundsSystem : JobComponentSystem
{
    public const float OUT_OF_BOUNDS_LIMIT = 10f;

    EntityCommandBufferSystem CommandBufferSystem;

    protected override void OnCreateManager()
    {
        CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = CommandBufferSystem.CreateCommandBuffer().ToConcurrent();

        var jobHandle = Entities
            .WithAll<Atom>()
            .ForEach(
            (Entity atom, int entityInQueryIndex, ref Translation translation) =>
                {
                    if (translation.Value.y < -OUT_OF_BOUNDS_LIMIT)
                        commandBuffer.DestroyEntity(entityInQueryIndex, atom);
                }
            )
            .Schedule(inputDeps);

        CommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}
