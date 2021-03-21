using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class StatisticsProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Statistics());
        dstManager.AddComponentData(entity, new MeanFreePath { Value = 0f });
        dstManager.AddComponentData(entity, new MeanCollisionTime { Value = 0f});

        DynamicBuffer<BufferElementPressure> dynamicBuffer = dstManager.AddBuffer<BufferElementPressure>(entity);
        
            // 20 Elements for the pressures on the faces
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 1f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 1f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 2f});


    }
}
