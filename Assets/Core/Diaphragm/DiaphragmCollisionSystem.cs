using Integration;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

/// <summary>
/// Collides particles that have been tagged to collide with the Diaphragm
/// </summary>
[UpdateBefore(typeof(ForceCalculationSystems))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class DiaphragmCollisionSystem : SystemBase
{
    EntityQuery DiaphragmQuery;

    protected override void OnCreate()
    {
        // Enabled = true;
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
        ComponentDataFromEntity<Velocity> allDiaphragmVelocity = GetComponentDataFromEntity<Velocity>(true);
        ComponentDataFromEntity<Mass> allDiaphragmMass = GetComponentDataFromEntity<Mass>(true);
            // Grab the component from the one diaphragm entity
        Velocity DiaphragmVelocity = allDiaphragmVelocity[tempEntity];
        Mass DiaphragmMass = allDiaphragmMass[tempEntity];
        
        float tempDiaphragmVelocityX = DiaphragmVelocity.Value.x;

        Entities.WithAll<Atom>().ForEach(
            (ref DiaphragmColliding diaphragmColliding,
            ref Velocity velocity,
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
