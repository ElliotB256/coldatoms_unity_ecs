using Integration;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Update Piston positions
/// </summary>
[UpdateBefore(typeof(PistonCollisionSystem))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class UpdatePistonPositionSystem : SystemBase
{
    float thermalisationTime = 10f;
    float initialXVel;
    bool holdVel = true;

    protected override void OnCreate()
    {
        // Enabled = true;
    }
    protected override void OnUpdate()
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        float CurrentTime = (float)Time.ElapsedTime;
        
        Entities.WithAll<Piston>().ForEach(
            (ref Translation translation, ref Velocity velocity, ref Oscillations oscillations) => {
                
                if (oscillations.CurrentOscillation == -1f) {
                    if (holdVel)
                    {
                        initialXVel = velocity.Value.x;
                        velocity.Value.x = 0f;
                        holdVel = false;
                    }
                    if (CurrentTime > thermalisationTime)
                    {
                        oscillations.CurrentOscillation = 0f;
                        velocity.Value.x = initialXVel;
                    }
                } else if (oscillations.CurrentOscillation <= oscillations.MaxOscillations) 
                {
                        // Bouncing the Piston back and forth to prevent 0 volume conditions
                        // Maybe have this in another system
                        translation.Value += velocity.Value * DeltaTime;
                    if (translation.Value.x < -9.999f || translation.Value.x > 0f)
                    {
                        velocity.Value *= -1;
                        oscillations.CurrentOscillation += 0.5f;
                    }
                } else {
                    velocity.Value.x = 0f;

                }
            }).WithoutBurst().Run();
    }
}


