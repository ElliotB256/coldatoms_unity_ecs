using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class AtomProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public float Mass = 1f;
    public float ScatteringRadius = 1f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Mass { Value = Mass });
        dstManager.AddComponentData(entity, new Velocity());
        dstManager.AddComponentData(entity, new Force());
        dstManager.AddComponentData(entity, new PrevForce());
        dstManager.AddComponentData(entity, new TotalEnergy());
        dstManager.AddComponentData(entity, new PotentialEnergy());
        dstManager.AddComponentData(entity, new KineticEnergy());
        dstManager.AddComponentData(entity, new CollisionRadius { Value = ScatteringRadius });
        dstManager.AddComponentData(entity, new Trapped());
        dstManager.AddComponentData(entity, new CollisionStats { TimeSinceLastCollision = 10f , DistanceSinceLastCollision = 0f, CollidedThisFrame = false});
        dstManager.AddComponentData(entity, new ShaderCollisionTime { Value = 100f });
        dstManager.AddComponentData(entity, new Atom());
            // Setting the Zone to -1 means that it is only changed once at the start
                // Work out how to make the system only run onces at the start (At least for this simple scene)
        dstManager.AddComponentData(entity, new Zone { Value = -1});
        dstManager.AddComponentData(entity, new DiaphragmColliding { Value = false});
        dstManager.AddComponentData(entity, new WallCollisions {Impulse = 0f, WallIndex = -1});
        dstManager.AddComponentData(entity, new LastFreePath { Value = 0f});
        dstManager.AddComponentData(entity, new LastFreeTime { Value = 0f});
        
    }
}
