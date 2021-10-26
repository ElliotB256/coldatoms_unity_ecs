using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateInGroup(typeof(ForceCalculationSystems))]
public class LaunchSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem BufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        BufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();

        if (sequence.Stage != SequenceStage.Launch)
            return;

        var buffer = BufferSystem.CreateCommandBuffer();

        //Prevent atoms from colliding, launch them.
        Dependency = Entities
            .WithAll<Trapped>()
            .ForEach(
            (Entity e, ref CollisionRadius radius, ref Velocity vel) =>
                { 
                    radius.Value = 0f;
                    //reduce velocity to allow us to actually see separation
                    vel.Value = vel.Value / 3f + new float3(0f, sequence.LaunchSpeed, 0f);
                    buffer.RemoveComponent<Trapped>(e);
                }
        )
        .Schedule(Dependency);

        BufferSystem.AddJobHandleForProducer(Dependency);

        sequence.Stage++;
        SetSingleton(sequence);
    }
}
