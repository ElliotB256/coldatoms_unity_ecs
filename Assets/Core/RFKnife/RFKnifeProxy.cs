using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class RFKnifeProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Radius of the RF knife.")]
    public float Radius = 1f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new RFKnife());
        dstManager.AddComponentData(entity, new Radius { Value = Radius });
        dstManager.AddComponentData(entity, new Scale());
        dstManager.AddComponentData(entity, new PlayerInputs());
    }
}
