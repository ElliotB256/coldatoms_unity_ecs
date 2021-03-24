using Integration;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Collides particles that have been tagged to collide with the Diaphragm
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class DiaphragmCollisionSystem : SystemBase
{
        // I don't think I am using this?
    EntityQuery DiaphragmQuery;

    protected override void OnCreate()
    {
        Enabled = false;
    }
    
    protected override void OnUpdate()
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        
            // Grabbing the diahragm entity
                // Is there an easier way to do this? Reference the entityManager?
        Entity tempEntity = Entity.Null;
        Entities.WithAll<Diaphragm>().ForEach((Entity entity)=> {
            tempEntity = entity;
        }).Run();

        // Make a native list of all the relevant components
            // Change this to just getting the single entity rather than the array then choosing the entity
        ComponentDataFromEntity<Velocity> allDiaphragmVelocity = GetComponentDataFromEntity<Velocity>(true);
        ComponentDataFromEntity<Mass> allDiaphragmMass = GetComponentDataFromEntity<Mass>(true);
        // ComponentDataFromEntity<WIndex> allDiaphragmWIndex = GetComponentDataFromEntity<WIndex>(true);
            // Grab the component from the one diaphragm entity
        Velocity DiaphragmVelocity = allDiaphragmVelocity[tempEntity];
        Mass DiaphragmMass = allDiaphragmMass[tempEntity];
        // WIndex DiaphragmIndex = allDiaphragmWIndex[tempEntity];
        
        float tempDiaphragmVelocityX = DiaphragmVelocity.Value.x;

        Entities.WithAll<Atom>().ForEach(
            (ref DiaphragmColliding diaphragmColliding,
            ref Velocity velocity,
            ref WallCollisions wallCollisions,
            in Translation translation,
            in Mass mass,
            in Zone zone) => 
        {
            if (diaphragmColliding.Value == true)
            {                    
                float CoMVelocityX = (tempDiaphragmVelocityX*DiaphragmMass.Value + mass.Value*velocity.Value.x)/(DiaphragmMass.Value + mass.Value);
                float particleCoMVelocityX = velocity.Value.x - CoMVelocityX;
                float diaphragmCoMVelocityX = tempDiaphragmVelocityX - CoMVelocityX;
                  
                particleCoMVelocityX *= -1;

                    // Update the particle impulse
                // wallCollisions.WallIndex = DiaphragmIndex.Value;
                // wallCollisions.Impulse = 2*mass.Value*Mathf.Abs(particleCoMVelocityX);

                velocity.Value.x = particleCoMVelocityX + CoMVelocityX;
                diaphragmColliding.Value = false;



                diaphragmCoMVelocityX -= 2*particleCoMVelocityX*mass.Value/DiaphragmMass.Value;
                tempDiaphragmVelocityX = diaphragmCoMVelocityX + CoMVelocityX;
            }
        }).Run();
        
            // Another ForEach to write back to the Diaphrahm
        Entities.WithAll<Diaphragm>().ForEach((ref Velocity velocity) => {
            velocity.Value.x = tempDiaphragmVelocityX;
        }).Run();
    }
}
