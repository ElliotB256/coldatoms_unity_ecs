using Unity.Entities;
using Unity.Mathematics;

public class FinalBeamSplitterSystem : SystemBase
{
    private EntityCommandBufferSystem BufferSystem;

    protected override void OnCreate()
    {
        BufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();

        if (sequence.Stage != SequenceStage.FinalBeamSplitter)
            return;

        var buffer = BufferSystem.CreateCommandBuffer();

        var random = new Random((uint)UnityEngine.Random.Range(0, 1000));

        Dependency = Entities
            .WithAll<Atom>()
            .ForEach(
            (Entity e, ref Velocity v, ref Phase phase, in Upper upper) =>
            {
                var chance = math.pow(math.sin(phase.Value), 2f);
                if (random.NextFloat(0f, 1f) > chance)
                {
                    buffer.DestroyEntity(e);
                } else
                {
                    buffer.DestroyEntity(upper.Lower);
                }
            }
        )
        .Schedule(Dependency);

        BufferSystem.AddJobHandleForProducer(Dependency);

        sequence.Stage++;

        SetSingleton(sequence);
    }
}
