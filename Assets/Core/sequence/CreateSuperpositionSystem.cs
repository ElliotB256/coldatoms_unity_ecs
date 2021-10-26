using Unity.Entities;
using Unity.Mathematics;

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
        

        //For each atom, create an excited state clone.
        Dependency = Entities
            .WithAll<Atom>()
            .ForEach(
            (Entity e, in Velocity v) =>
            {
                var clone = buffer.Instantiate(e);
                buffer.AddComponent(clone, new Upper { Lower = e });
                buffer.AddComponent(clone, new Phase { Value = startingPhase });
                buffer.SetComponent(clone, new Velocity { Value = v.Value + new float3(0f, sequence.PhotonRecoilVelocity, 0f) });
            }
        )
        .Schedule(Dependency);

        BufferSystem.AddJobHandleForProducer(Dependency);

        sequence.Stage++;

        SetSingleton(sequence);
    }
}
