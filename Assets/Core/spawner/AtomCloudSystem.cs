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

    static bool spawnSpecial = false;

    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    struct SpawnJob : IJobForEachWithEntity<AtomCloud, LocalToWorld>
    {
        [ReadOnly] public Random Random;
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref AtomCloud cloud,
            [ReadOnly] ref LocalToWorld location)
        {
            if (!cloud.ShouldSpawn)
                return;
            
            if (spawnSpecial)
            {
                // Spawn special atoms to test features
                // for (int i = 0; i < 2; i++)
                // {
                    // Special 1
                    var instance = CommandBuffer.Instantiate(index, cloud.Atom);
                    var position = new float3(-6.2f, 0.67f, 0f);
                    CommandBuffer.SetComponent(index, instance, new Translation { Value = position});
                    var velocity = new float3(4f, 0f, 0f);
                    CommandBuffer.SetComponent(index, instance, new Velocity { Value = velocity });
                    
                    // Special 2
                    instance = CommandBuffer.Instantiate(index, cloud.Atom);
                    position = new float3(-2.2f, 0f, 0f);
                    CommandBuffer.SetComponent(index, instance, new Translation { Value = position});
                    velocity = new float3(-4f, 0f, 0f);
                    CommandBuffer.SetComponent(index, instance, new Velocity { Value = velocity });
                // }
            } else 
            {
                // Spawning the main cloud (for indicies greater than 2)
                for (int i = 0; i < cloud.Number; i++)
                {
                var instance = CommandBuffer.Instantiate(index, cloud.Atom);

                    // Place the instantiated in a grid with some noise
                // var theta = Random.NextFloat(0f, 1f) * math.PI;
                // var phi = Random.NextFloat(0f, 1f) * 2 * math.PI;
                // var r = Random.NextFloat(0f, cloud.Radius);

                    // conditional operator ?: (condition ? consequent : alternative)
                var zScale = cloud.ThreeDimensions ? 1.0f : 0.0f;

                // var position = new float3(
                // r * math.sin(theta) * math.cos(phi),
                // r * math.sin(theta) * math.sin(phi),
                //     // Here the z extent is r or 0 depending on the cloud.ThreeDimensions condidition
                // zScale*r * math.cos(theta)
                // );

                    // Can choose this for filling the box.
                var x = cloud.Radius*(-1f + Random.NextFloat(0f, 1f)*2f);
                var y = cloud.Radius*(-1f + Random.NextFloat(0f, 1f)*2f);
                var z = zScale*cloud.Radius*(-1f + Random.NextFloat(0f, 1f)*2f);

                var position = new float3(x, y, z);


                CommandBuffer.SetComponent(index, instance, new Translation { Value = position + location.Position });

                    // Give random velocities
                var velocity = new float3(
                    Random.NextFloat(-1f, 1f),
                    Random.NextFloat(-1f, 1f),
                    zScale*Random.NextFloat(-1f, 1f)
                ) * cloud.SpawnVelocities + cloud.COMVelocity;

                // if (x < 0)
                // {
                //     velocity *= 2;
                // }

                CommandBuffer.SetComponent(index, instance, new Velocity { Value = velocity });
                
                }
            }

            cloud.ShouldSpawn = false;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new SpawnJob
        {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                // Does the 10000 limit the number of unique positions/velocities?
                    // I thought the second argument was the upper limit 
            Random = new Random((uint)UnityEngine.Random.Range(1, 10000))
        }.Schedule(this, inputDeps);
        m_EntityCommandBufferSystem.AddJobHandleForProducer(job);
        return job;
    }
}