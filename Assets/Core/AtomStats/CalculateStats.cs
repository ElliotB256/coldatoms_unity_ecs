using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Calculation
{
[UpdateInGroup(typeof(CalculationSystemGroup))]
// [UpdateAfter(typeof(CalculateDistanceSinceLastCollision))]
public class CalculateStats : SystemBase
{
    protected override void OnUpdate()
    {
        
        Entities.ForEach((ref CollisionStats stats, ref LastFreePath lastFreePath, ref LastFreeTime lastFreeTime, in Velocity velocity) => {
            if (stats.CollidedThisFrame)
            {
                stats.DistanceSinceLastCollision = stats.TimeSinceLastCollision*math.length(velocity.Value);

                lastFreePath.Value = stats.DistanceSinceLastCollision;
                lastFreeTime.Value = stats.TimeSinceLastCollision;
                stats.TimeSinceLastCollision = 0f;
                stats.CollidedThisFrame = false;
            }
        }).Schedule();
    }
}
}