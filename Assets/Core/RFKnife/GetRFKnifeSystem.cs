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
    public bool KnifeExists;

    protected override void OnUpdate()
    {
        KnifeExists = false;
        Entities
            .With(RFKnives)
            .ForEach(
                (Entity e, ref Radius r, ref Translation t) =>
                {
                    Radius = r.Value;
                    Position = t.Value;
                    KnifeExists = true;
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
