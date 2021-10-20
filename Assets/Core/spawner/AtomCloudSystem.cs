using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// Spawns atoms at AtomCloud entities.
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class AtomCloudSystem : SystemBase
{
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var buffer = m_EntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var random = new Random((uint)UnityEngine.Random.Range(1, 10000));

        Dependency = Entities
            .ForEach(
            (Entity entity, int entityInQueryIndex, ref AtomCloud cloud,
            in LocalToWorld location) =>
            {

                if (!cloud.ShouldSpawn)
                    return;

                // Spawning the main cloud (for indicies greater than 2)
                for (int i = 0; i < cloud.Number; i++)
                {
                    var atomEntity = buffer.Instantiate(entityInQueryIndex, cloud.Atom);
                    var phi = random.NextFloat(0f, 2f * math.PI);
                    var theta = random.NextFloat(0f, math.PI);
                    var x = cloud.Radius * math.sin(theta) * math.cos(phi);
                    var y = cloud.Radius * math.sin(theta) * math.sin(phi);
                    var z = cloud.Radius * math.cos(theta);

                    var position = new float3(x, y, z);


                    buffer.SetComponent(entityInQueryIndex, atomEntity, new Translation { Value = position + location.Position });

                    // Give random velocities
                    var velocity = new float3(
                        random.NextFloat(-1f, 1f),
                        random.NextFloat(-1f, 1f),
                        random.NextFloat(-1f, 1f)
                    ) * cloud.SpawnVelocities + cloud.COMVelocity;

                    buffer.SetComponent(entityInQueryIndex, atomEntity, new Velocity { Value = velocity });
                }
                cloud.ShouldSpawn = false;
            }
            )
            .Schedule(Dependency);

        m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}