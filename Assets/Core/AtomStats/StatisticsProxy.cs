using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class StatisticsProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    // [SerializeField]
    // private float MaxOscillationsNumber = 9.5f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Statistics());

        dstManager.AddComponentData(entity, new MeanFreePath { Value = 0f });
        dstManager.AddComponentData(entity, new MeanCollisionTime { Value = 0f});
        
        dstManager.AddComponentData(entity, new TotalVolume { Value = 0});
        dstManager.AddComponentData(entity, new Z0Volume { Value = 0});
        dstManager.AddComponentData(entity, new Z1Volume { Value = 0});
        dstManager.AddComponentData(entity, new Z2Volume { Value = 0});
        
        dstManager.AddComponentData(entity, new TotalInternalEnergy { Value = 0f});
        dstManager.AddComponentData(entity, new UZ0 { Value = 0});
        dstManager.AddComponentData(entity, new UZ1 { Value = 0});
        dstManager.AddComponentData(entity, new UZ2 { Value = 0});
        
        dstManager.AddComponentData(entity, new Number { Value = 0});
        dstManager.AddComponentData(entity, new NZ0 { Value = 0});
        dstManager.AddComponentData(entity, new NZ1 { Value = 0});
        dstManager.AddComponentData(entity, new NZ2 { Value = 0});

        dstManager.AddComponentData(entity, new TZ0 { Value = 0});
        dstManager.AddComponentData(entity, new TZ1 { Value = 0});
        dstManager.AddComponentData(entity, new TZ2 { Value = 0});

        dstManager.AddComponentData(entity, new Oscillations { CurrentOscillation = 0f, MaxOscillations = 0f});

        

        DynamicBuffer<BufferElementPressure> dynamicBuffer = dstManager.AddBuffer<BufferElementPressure>(entity);
        
            // 20 Elements for the pressures on the faces
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});
        dynamicBuffer.Add(new BufferElementPressure { Value = 0f});


    }
}
