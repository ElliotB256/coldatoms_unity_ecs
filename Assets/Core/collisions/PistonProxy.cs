using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PistonProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Initial velocity of the piston.")]
    public Vector3 Velocity = new Vector3(-0.1f, 0f, 0f);

    [Tooltip("Mass of the piston")]
    public float Mass = 100f;

    [Tooltip("Initial position of the piston")]
    public Vector3 Position = new Vector3(10f, 0f, 0f);//transform.position;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Piston { Position = Position, Velocity = Velocity, Mass = Mass });
    }
}