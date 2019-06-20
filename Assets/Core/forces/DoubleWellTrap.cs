using Unity.Entities;

/// <summary>
/// A double-well trap
/// </summary>
public struct DoubleWellTrap : IComponentData
{
    /// <summary>
    /// Separation between the two wells
    /// </summary>
    public float Separation;

    /// <summary>
    /// Spring constant of the trap
    /// </summary>
    public float SpringConstant;

    /// <summary>
    /// x^4 coefficient
    /// </summary>
    public float alpha;
}
