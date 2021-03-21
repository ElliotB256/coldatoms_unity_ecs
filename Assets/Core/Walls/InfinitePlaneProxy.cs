using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class InfinitePlaneProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField]
    private int index = -1;
    [SerializeField]
    private int Z1index = -1;
    [SerializeField]
    private int Z2index = -1;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new InfinitePlane { V1 = transform.position, Normal = transform.forward });
        dstManager.AddComponentData(entity, new Pressure {Value = 0});
        dstManager.AddComponentData(entity, new PressureZ1 {Value = 0});
        dstManager.AddComponentData(entity, new PressureZ2 {Value = 0});
        dstManager.AddComponentData(entity, new Wall());
        dstManager.AddComponentData(entity, new WIndex{ Value = index});
        dstManager.AddComponentData(entity, new WZ1Index{ Value = Z1index});
        dstManager.AddComponentData(entity, new WZ2Index{ Value = Z2index});
    }
}
