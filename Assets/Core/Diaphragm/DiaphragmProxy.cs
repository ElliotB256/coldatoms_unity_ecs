using System;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class DiaphragmProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Initial velocity of the Diaphragm.")]
    // [Serializable]
    private float3 initialVelocity = new float3(0f, 0f, 0f);

    [Tooltip("Mass of the Diaphragm")]
    // [Serializable]
    private float mass = 100f;

    [SerializeField]
    private int index = -1;
    [SerializeField]
    private int indexLeft = -1;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Mass { Value = mass});
        dstManager.AddComponentData(entity, new Velocity() {Value = initialVelocity});
        dstManager.AddComponentData(entity, new Diaphragm());
        dstManager.AddComponentData(entity, new WIndex{ Value = index});
        dstManager.AddComponentData(entity, new WIndexLeft { Value = indexLeft});
        dstManager.AddComponentData(entity, new Pressure {Value = 0});
        dstManager.AddComponentData(entity, new PressureLeft { Value = 0});
    }
}