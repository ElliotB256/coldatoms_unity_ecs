using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PistonProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Initial velocity of the piston.")]
        // Not sure if this attribute works with a vector3?
    [SerializeField]
    private Vector3 initialVelocity = new Vector3(-0.2f, 0f, 0f);

    [Tooltip("Mass of the piston")]
    // Need to correctly implement infinite mass in the system
        // Currently using CoM to ensure the velocity check works
    [SerializeField]
    private float mass = 1000000f;

    [SerializeField]
    private float MaxOscillationsNumber = 9.5f;
    [SerializeField]
    private float MaxDistance = 2f;
    
    [SerializeField]
    private int index = -1;
    [SerializeField]
    private int indexLeft = -1;
    // [Tooltip("Initial position of the piston")]
    // public Vector3 initialPosition = new Vector3(10f, 0f, 0f);//transform.position;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Mass { Value = mass});
        dstManager.AddComponentData(entity, new Velocity() {Value = initialVelocity});
        dstManager.AddComponentData(entity, new Piston());
        dstManager.AddComponentData(entity, new WIndex{ Value = index});
        dstManager.AddComponentData(entity, new WIndexLeft { Value = indexLeft});
        dstManager.AddComponentData(entity, new Pressure {Value = 0});
        dstManager.AddComponentData(entity, new PressureLeft { Value = 0});
        dstManager.AddComponentData(entity, new Oscillations { CurrentOscillation = -1f, MaxOscillations = MaxOscillationsNumber});
        dstManager.AddComponentData(entity, new MaxDistance {Value = MaxDistance });
    }
}