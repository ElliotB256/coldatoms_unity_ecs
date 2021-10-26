using Integration;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedUpdateGroup))]
public class MakeMeasurementSystem : SystemBase
{
    protected override void OnCreate()
    {
        lowerQ = GetEntityQuery(ComponentType.ReadOnly<Atom>(), ComponentType.Exclude<Upper>());
        upperQ = GetEntityQuery(ComponentType.ReadOnly<Atom>(), ComponentType.ReadOnly<Upper>());
    }

    EntityQuery upperQ;
    EntityQuery lowerQ;

    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();

        if (sequence.Stage != SequenceStage.MakeMeasurement)
            return;

        var upper = (float)upperQ.CalculateEntityCount();
        var lower = (float)lowerQ.CalculateEntityCount();

        var reading = (upper - lower) / (upper + lower) * 2f * 10f;

        var newPoint = EntityManager.Instantiate(sequence.GraphPointTemplate);
        EntityManager.SetComponentData(newPoint, new Translation { Value = new float3 { x = 20f + sequence.StartingTime / 6f * 6f, y = 15f + reading, z = 0f } });

        sequence.Stage++;
        SetSingleton(sequence);
    }
}
