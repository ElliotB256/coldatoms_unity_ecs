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
public class AtomCloudSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    struct SpawnJob : IJobForEachWithEntity<AtomCloud, LocalToWorld>
    {
        [ReadOnly] public Random Random;
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref AtomCloud cloud,
            [ReadOnly] ref LocalToWorld location)
        {
            if (!cloud.ShouldSpawn)
                return;

            for (int i = 0; i < cloud.Number; i++)
            {
                var instance = CommandBuffer.Instantiate(index, cloud.Atom);

                // Place the instantiated in a grid with some noise
                var theta = Random.NextFloat(0f, 1f) * math.PI;
                var phi = Random.NextFloat(0f, 1f) * 2 * math.PI;
                var r = Random.NextFloat(0f, cloud.Radius);
                var zScale = cloud.ThreeDimensions ? 1.0f : 0.0f;
                var position = new float3(
                    r * math.sin(theta) * math.cos(phi),
                    r * math.sin(theta) * math.sin(phi),
                    zScale*r * math.cos(theta)
                    );
                CommandBuffer.SetComponent(index, instance, new Translation { Value = position + location.Position });

                // Give random velocities
                var velocity = new float3(
                    Random.NextFloat(-1f, 1f),
                    Random.NextFloat(-1f, 1f),
                    zScale*Random.NextFloat(-1f, 1f)
                ) * cloud.SpawnVelocities + cloud.COMVelocity;
                CommandBuffer.SetComponent(index, instance, new Velocity { Value = velocity });
                
            }

            cloud.ShouldSpawn = false;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new SpawnJob
        {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            Random = new Random((uint)UnityEngine.Random.Range(1, 10000))
        }.Schedule(this, inputDeps);
        m_EntityCommandBufferSystem.AddJobHandleForProducer(job);
        return job;
    }
}