using Integration;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

/// <summary>
/// Deletes any atoms that leave the simulation bounds.
/// Altered this to bring the atoms back into the box
    /// this should be changed in the presence of a potential
/// </summary>
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class OutOfBoundsSystem : JobComponentSystem
{
    public const float OUT_OF_BOUNDS_LIMIT = 10.1f;

    // EntityCommandBufferSystem CommandBufferSystem;

    protected override void OnCreate()
    {
        // CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // var commandBuffer = CommandBufferSystem.CreateCommandBuffer().ToConcurrent();

        // var jobHandle = 
        return Entities
            .WithAll<Atom>()
            .ForEach(
            (Entity atom, int entityInQueryIndex, ref Translation translation) =>
                {
                    if (translation.Value.y < -OUT_OF_BOUNDS_LIMIT)
                    {
                        translation.Value.y = -9.9f;
                        // commandBuffer.DestroyEntity(entityInQueryIndex, atom);
                    }
                    if (translation.Value.y > OUT_OF_BOUNDS_LIMIT)
                    {
                        translation.Value.y = 9.9f;
                    }

                    if (translation.Value.x < -OUT_OF_BOUNDS_LIMIT)
                    {
                        translation.Value.x = -9.9f;
                    }
                    if (translation.Value.x > OUT_OF_BOUNDS_LIMIT)
                    {
                        translation.Value.x = 9.9f;
                    }
                    if (translation.Value.z < -OUT_OF_BOUNDS_LIMIT)
                    {
                        translation.Value.z = -9.9f;
                    }
                    if (translation.Value.z > OUT_OF_BOUNDS_LIMIT)
                    {
                        translation.Value.z = 9.9f;
                    }
                }
            )
            .Schedule(inputDeps);

        // CommandBufferSystem.AddJobHandleForProducer(jobHandle);
        // return jobHandle;
    }
}
