﻿using Unity.Burst;
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
            for (int i = 0; i < cloud.Number; i++)
            {
                var instance = CommandBuffer.Instantiate(index, cloud.Atom);

                // Place the instantiated in a grid with some noise
                var theta = Random.NextFloat(0f, 1f) * math.PI;
                var phi = Random.NextFloat(0f, 1f) * 2 * math.PI;
                var r = Random.NextFloat(0f, cloud.Radius);
                var position = new float3(
                    r * math.sin(theta) * math.cos(phi),
                    r * math.sin(theta) * math.sin(phi),
                    r * math.cos(theta)
                    );
                CommandBuffer.SetComponent(index, instance, new Translation { Value = position });
            }

            CommandBuffer.DestroyEntity(index, entity);
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