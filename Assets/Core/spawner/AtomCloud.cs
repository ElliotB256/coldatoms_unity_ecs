using Unity.Entities;

/// <summary>
/// Spawns an atom cloud
/// </summary>
public struct AtomCloud : IComponentData
{
    public Entity Atom;
    public int Number;
    public float Radius;
}
