using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class HarmonicTrapProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Spring constant of this harmonic trap.")]
    public float SpringConstant = 1f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new HarmonicTrap { SpringConstant = SpringConstant });
    }
}
