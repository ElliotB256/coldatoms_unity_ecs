using Integration;
using Unity.Entities;
using Unity.Jobs;

[UpdateAfter(typeof(UpdateVelocitySystem))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class SavePrevForceSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach(
            (ref PrevForce prevForce, in Force force) =>
                prevForce.Value = force.Value
                ).Schedule();
    }
}