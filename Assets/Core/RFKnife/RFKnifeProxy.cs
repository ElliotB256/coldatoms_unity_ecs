using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class RFKnifeProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public float Radius = 1f;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Radius { Value = Radius });
        dstManager.AddComponentData(entity, new PlayerInputs {
            UpButton = new Boolean(false),
            DownButton = new Boolean(false)
        }
        );
    }
}
