using Integration;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Finds the particles that are colliding with the Diaphragm this frame
/// </summary>
[UpdateBefore(typeof(DiaphragmCollisionSystem))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class DiaphragmCollisionDetectionSystem : JobComponentSystem
{
    EntityQuery DiaphragmQuery;
    EntityQuery PistonQuery;

    protected override void OnCreate()
    {
        // Enabled = false;

        var diaphragmQueryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Velocity>(),
                ComponentType.ReadOnly<Mass>(),
                ComponentType.ReadOnly<Diaphragm>()                
            }
        };
        DiaphragmQuery = GetEntityQuery(diaphragmQueryDesc);

        var pistonQueryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<Piston>(),
                ComponentType.ReadOnly<Translation>()
            }
        };
        PistonQuery = GetEntityQuery(pistonQueryDesc);
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;

            // I can't use the same logic as DiaphragmCollisionSystem to get the diaphragm components as that needs to be .Run() to get the entity reference 
                // Surely there is an easier and better way that this?
                    // Get singleton and getComponentData
        NativeArray<Translation> DiaphragmTranslation = DiaphragmQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<Velocity> DiaphragmVelocity = DiaphragmQuery.ToComponentDataArray<Velocity>(Allocator.TempJob);
        NativeArray<Mass> DiaphragmMass = DiaphragmQuery.ToComponentDataArray<Mass>(Allocator.TempJob);

        NativeArray<Translation> PistonList = PistonQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        int PistonListLength = PistonList.Length;

        var firstJob = Entities
            .WithAll<Atom>()
            .ForEach(
                (ref DiaphragmColliding diaphragmColliding,
                ref WallCollisions wallCollisions,
                ref CollisionStats stats,
                in Translation translation,
                in Velocity velocity,
                in Mass mass,
                in Zone zone,
                in CollisionRadius collisionRadius) => 
                {
                    if (DiaphragmTranslation.Length > 0) {
                    // If inbetween the Piston and the Diaphragm
                    if (zone.Value == PistonListLength)
                    {
                        if (translation.Value.x > DiaphragmTranslation[0].Value.x - collisionRadius.Value) {

                            float3 CoMVelocity = (DiaphragmMass[0].Value * DiaphragmVelocity[0].Value + mass.Value*velocity.Value)/(DiaphragmMass[0].Value + mass.Value);
                            float3 particleCoMVelocity = velocity.Value - CoMVelocity;
                        
                            if (particleCoMVelocity.x > 0f) {
                                    // Tag this particle for colliding with the diaphragm
                                diaphragmColliding.Value = true;
                                stats.CollidedThisFrame = true;
                                
                                    // Update the wallCollisions component
                                wallCollisions.WallIndex = 9;
                                wallCollisions.Impulse = 2*mass.Value * Mathf.Abs(particleCoMVelocity.x);
                            }
                        }
                    } else if (zone.Value == PistonListLength + 1)
                    {
                        // if inbetween the diaphragm and the right hand wall
                        if (translation.Value.x < DiaphragmTranslation[0].Value.x + collisionRadius.Value) {
                            float3 CoMVelocity = (DiaphragmMass[0].Value * DiaphragmVelocity[0].Value + mass.Value*velocity.Value)/(DiaphragmMass[0].Value + mass.Value);
                            float3 particleCoMVelocity = velocity.Value - CoMVelocity;
                        
                            if (particleCoMVelocity.x < 0f) {
                                diaphragmColliding.Value = true;

                                wallCollisions.WallIndex = 8;
                                wallCollisions.Impulse = 2*mass.Value * Mathf.Abs(particleCoMVelocity.x);
                            }
                        }
                    }   
                    }
                }).Schedule(inputDependencies);
        
        DiaphragmTranslation.Dispose(firstJob);
        DiaphragmVelocity.Dispose(firstJob);
        DiaphragmMass.Dispose(firstJob);
        PistonList.Dispose(firstJob);

        return firstJob;
    }
}


// Make an array of diaphragmTranslations and velocities
// ForEach through the particles
// For each zone about the diaphragm, detect if a particle is coming in for a collision
// Add that particle index to a list or set its component value to colliding? (for more than one diaphragm I will need to know which it is colliding with)