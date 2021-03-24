using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

// Run this at the start of each frame.
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(PlayerInputSystem))]
public class SetPreviousVelocity : SystemBase
{
    protected override void OnUpdate()
    {   
        Entities.WithAll<Atom>().ForEach((ref PrevVelocity prevVel, in Velocity velocity) => {
            prevVel.Value = velocity.Value;
        }).Schedule();
    }
}
