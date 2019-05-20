using Unity.Entities;

/// <summary>
/// Untraps particles that are more than 'radius' away from transform.
/// </summary>
public struct Untrapper : IComponentData
{
    public float Radius;
}
