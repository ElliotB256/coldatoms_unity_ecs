using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class DoubleWellTrapProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Spring constant of this trap.")]
    public float SpringConstant = 1f;

    public float Separation = 0f;

    public float Alpha = 1f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new DoubleWellTrap { SpringConstant = SpringConstant, Separation = Separation, alpha = Alpha });
    }
}
