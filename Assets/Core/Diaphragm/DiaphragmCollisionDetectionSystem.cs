using Integration;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

/// <summary>
/// Finds the particles that are colliding with the Diaphragm this frame
/// </summary>
[UpdateBefore(typeof(DiaphragmCollisionSystem))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class DiaphragmCollisionDetectionSystem : JobComponentSystem
{
    EntityQuery DiaphragmQuery;


    protected override void OnCreate()
    {
        // Enabled = true;

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
    }
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;

            // I can't use the same logic as DiaphragmCollisionSystem to get the diaphragm components as that needs to be .Run() to get the entity reference 
                // Surely there is an easier and better way that this?
        NativeArray<Translation> DiaphragmTranslation = DiaphragmQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<Velocity> DiaphragmVelocity = DiaphragmQuery.ToComponentDataArray<Velocity>(Allocator.TempJob);
        NativeArray<Mass> DiaphragmMass = DiaphragmQuery.ToComponentDataArray<Mass>(Allocator.TempJob);

        return Entities
            .WithAll<Atom>()
            .ForEach(
                (ref DiaphragmColliding diaphragmColliding,
                in Translation translation,
                in Velocity velocity,
                in Mass mass,
                in Zone zone,
                in CollisionRadius collisionRadius) => 
                {                
                    // If inbetween the Piston and the Diaphragm
                    if (zone.Value == 1)
                    {
                        if (translation.Value.x > DiaphragmTranslation[0].Value.x - collisionRadius.Value) {

                            float3 CoMVelocity = (DiaphragmMass[0].Value * DiaphragmVelocity[0].Value + mass.Value*velocity.Value)/(DiaphragmMass[0].Value + mass.Value);
                            float3 particleCoMVelocity = velocity.Value - CoMVelocity;
                        
                            if (particleCoMVelocity.x > 0f) {
                                diaphragmColliding.Value = true;
                            }
                        }
                    }
                        // if inbetween the diaphragm and the right hand wall
                    else if (zone.Value == 2)
                    {
                        if (translation.Value.x < DiaphragmTranslation[0].Value.x + collisionRadius.Value) {
                            float3 CoMVelocity = (DiaphragmMass[0].Value * DiaphragmVelocity[0].Value + mass.Value*velocity.Value)/(DiaphragmMass[0].Value + mass.Value);
                            float3 particleCoMVelocity = velocity.Value - CoMVelocity;
                        
                            if (particleCoMVelocity.x < 0f) {
                                diaphragmColliding.Value = true;
                            }
                        }
                    }   

            }).Schedule(inputDependencies);
    }
}


// Make an array of diaphragmTranslations and velocities
// ForEach through the particles
// For each zone about the diaphragm, detect if a particle is coming in for a collision
// Add that particle index to a list or set its component value to colliding? (for more than one diaphragm I will need to know which it is colliding with)