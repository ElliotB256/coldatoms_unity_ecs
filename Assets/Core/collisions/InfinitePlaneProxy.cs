using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class InfinitePlaneProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new InfinitePlane { V1 = transform.position, Normal = transform.forward });
        dstManager.AddComponentData(entity, new Impulse { Value = 0f});
    }
}
