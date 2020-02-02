using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Spawns an atom cloud
/// </summary>
public struct AtomCloud : IComponentData
{
    public Entity Atom;
    public int Number;
    public float Radius;
    public float SpawnVelocities;
    public float3 COMVelocity;
    public bool ThreeDimensions;
    public bool ShouldSpawn;
}
