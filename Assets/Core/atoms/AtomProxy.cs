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
        dstManager.AddComponentData(entity, new CollisionRadius { Value = ScatteringRadius });
        dstManager.AddComponentData(entity, new Trapped());
        dstManager.AddComponentData(entity, new CollisionStats { TimeSinceLastCollision = 10f });
        dstManager.AddComponentData(entity, new ShaderCollisionTime { Value = 100f });
        dstManager.AddComponentData(entity, new Atom());
    }
}
