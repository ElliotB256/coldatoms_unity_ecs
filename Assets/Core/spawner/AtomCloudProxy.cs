using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AtomCloudProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    [Tooltip("Entity type spawned by the cloud.")]
    public GameObject Archetype;

    [Tooltip("Radius of atom cloud.")]
    public float Radius = 10f;

    [Tooltip("Number of atoms to spawn.")]
    public int Number = 10;

    [Tooltip("Velocity scale of spawned atoms.")]
    public float SpawnVelocities = 1f;

    public Vector3 CentreOfMassVelocity;

    public bool UseThreeDimensions = true;

    public bool ShouldSpawn = true;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var prefab = conversionSystem.GetPrimaryEntity(Archetype);
        var atomCloud = new AtomCloud
        {
            Atom = prefab,
            Radius = Radius,
            Number = Number,
            SpawnVelocities = SpawnVelocities,
            COMVelocity = CentreOfMassVelocity,
            ThreeDimensions = UseThreeDimensions,
            ShouldSpawn = ShouldSpawn
        };
        dstManager.AddComponentData(entity, atomCloud);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Archetype);
    }
}