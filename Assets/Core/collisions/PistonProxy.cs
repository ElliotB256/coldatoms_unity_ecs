using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PistonProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Initial velocity of the piston.")]
    public Vector3 initialVelocity = new Vector3(-0.1f, 0f, 0f);

    [Tooltip("Mass of the piston")]
    public float mass = 100f;

    // [Tooltip("Initial position of the piston")]
    // public Vector3 initialPosition = new Vector3(10f, 0f, 0f);//transform.position;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Mass { Value = mass});
        dstManager.AddComponentData(entity, new Velocity() {Value = initialVelocity});
        dstManager.AddComponentData(entity, new Piston
        {
            Translation = transform.position, 
            Velocity = initialVelocity, 
            Mass = mass
        }
        );
    }
}