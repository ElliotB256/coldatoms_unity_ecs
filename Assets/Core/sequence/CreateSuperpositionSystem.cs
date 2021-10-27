using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class CreateSuperpositionsystem : SystemBase
{
    private EntityCommandBufferSystem BufferSystem;

    protected override void OnCreate()
    {
        BufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();

        if (sequence.Stage != SequenceStage.CreateSuperposition)
            return;

        var buffer = BufferSystem.CreateCommandBuffer();

        var period = 60f;
        sequence.StartingTime = (float)Time.ElapsedTime % period;
        float startingPhase = sequence.StartingTime / period * 2f * math.PI;
        float r02 = math.pow(sequence.BeamRadius, 2f);

        //For each atom, create an excited state clone.
        Dependency = Entities
            .WithAll<Atom>()
            .ForEach(
            (Entity e, in Velocity v, in Translation t) =>
            {
                var clone = buffer.Instantiate(e);
                var r2 = math.pow(t.Value.x, 2f) + math.pow(t.Value.z, 2f);
                var theta = math.PI / 2f * math.exp(-r2 / r02);
                buffer.AddComponent(clone, new Upper { Lower = e, Theta1 = theta, Theta2 = 0f });
                buffer.AddComponent(clone, new Phase { Value = startingPhase });
                buffer.SetComponent(clone, new Velocity { Value = v.Value + new float3(0f, sequence.PhotonRecoilVelocity, 0f) });
            }
        )
        .Schedule(Dependency);

        Entities.ForEach((ref BeamPower power) => power.Value = 0.1f).Schedule();

        BufferSystem.AddJobHandleForProducer(Dependency);

        sequence.Stage++;

        SetSingleton(sequence);
    }
}
