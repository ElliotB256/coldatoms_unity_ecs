using Unity.Entities;
using Unity.Jobs;

namespace Calculation
{
    [UpdateInGroup(typeof(CalculationSystemGroup))]
public class UpdateTimeSinceLastCollisionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref CollisionStats stats) => {
            if (stats.CollidedThisFrame)
            {
                stats.TimeSinceLastCollision = 0f;
                stats.CollidedThisFrame = false;
            }
        }).Schedule();
    }
}
}