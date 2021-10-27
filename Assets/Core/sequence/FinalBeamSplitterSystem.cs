using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
        float r02 = math.pow(sequence.BeamRadius, 2f);

        Dependency = Entities
            .WithAll<Atom>()
            .ForEach(
            (Entity e, ref Velocity v, ref Phase phase, ref Upper upper, in Translation t) =>
            {
                var chance = math.pow(math.sin(phase.Value), 2f);

                //// Use beam waist imperfection
                //var r2 = math.pow(t.Value.x, 2f) + math.pow(t.Value.z, 2f);
                //var theta2 = math.PI / 2f * math.exp(-r2 / r02);
                //chance = 0.5f * (1f - math.cos(upper.Theta1) * math.cos(theta2) + math.cos(phase.Value) * math.sin(upper.Theta1) * math.sin(theta2));

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

        Entities.ForEach((ref BeamPower power) => power.Value = 0.1f).Schedule();

        BufferSystem.AddJobHandleForProducer(Dependency);

        sequence.Stage++;

        SetSingleton(sequence);
    }
}
