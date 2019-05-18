using Unity.Entities;

/// <summary>
/// A harmonic trap confines atoms with a force proportional to -kX
/// </summary>
public struct HarmonicTrap : IComponentData
{
    /// <summary>
    /// Spring constant of the harmonic trap
    /// </summary>
    public float SpringConstant;
}
