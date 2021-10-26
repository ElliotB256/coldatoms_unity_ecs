using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
class TabularRFKnifeProxy : MonoBehaviour, IConvertGameObjectToEntity
{

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<TabularRFKnifeElement>(entity);
        dstManager.AddComponentData(entity, new TabularRFKnife{CurrentTime = 0f});
        
        var buffer = dstManager.GetBuffer<TabularRFKnifeElement>(entity);
        buffer.Add(new TabularRFKnifeElement { Duration = 0.0f, Radius = 10f });
        buffer.Add(new TabularRFKnifeElement { Duration = 4.0f, Radius = 9.5f });
        buffer.Add(new TabularRFKnifeElement { Duration = 4.0f, Radius = 6f });
        buffer.Add(new TabularRFKnifeElement { Duration = 4.0f, Radius = 4.5f });
        buffer.Add(new TabularRFKnifeElement { Duration = 4.0f, Radius = 3f });
    }
}
