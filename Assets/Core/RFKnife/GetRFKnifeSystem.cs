using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// Locates rf knife and exports values for other systems to use.
/// </summary>
[UpdateBefore(typeof(RFKnifeSystem))]
public class GetRFKnifeSystem : ComponentSystem
{
    public float Radius;
    public float3 Position;

    protected override void OnUpdate()
    {
        Entities
            .With(RFKnives)
            .ForEach(
                (Entity e, ref Radius r, ref Translation t) =>
                {
                    Radius = r.Value;
                    Position = t.Value;
                }
        );
    }

    private EntityQuery RFKnives;

    protected override void OnCreateManager()
    {
        RFKnives = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                    ComponentType.ReadOnly<RFKnife>()
                }
        }
        );
    }
}
