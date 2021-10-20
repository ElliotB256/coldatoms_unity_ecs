using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
class TabularRFKnifeProxy : MonoBehaviour, IConvertGameObjectToEntity
{

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<TabularRFKnifeElement>(entity);
        dstManager.AddComponentData<TabularRFKnife>(entity, new TabularRFKnife{CurrentTime = 0f});
        
        var buffer = dstManager.GetBuffer<TabularRFKnifeElement>(entity);
        buffer.Add(new TabularRFKnifeElement { Duration = 0.0f, Radius = 10f });
        buffer.Add(new TabularRFKnifeElement { Duration = 10.0f, Radius = 9f });
        buffer.Add(new TabularRFKnifeElement { Duration = 5.0f, Radius = 7.5f });
        buffer.Add(new TabularRFKnifeElement { Duration = 5.0f, Radius = 5f });
        buffer.Add(new TabularRFKnifeElement { Duration = 5.0f, Radius = 4f });
    }
}
