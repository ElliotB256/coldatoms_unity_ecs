using System;
using Unity.Entities;

/// <summary>
/// Kinetic energy associated with an entity.
/// </summary>
[Serializable]
public struct KineticEnergy : IComponentData
{
    public float Value;
}

/// <summary>
/// Potential Energy associated with an entity.
/// </summary>
[Serializable]
public struct PotentialEnergy : IComponentData
{
    public float Value;
}

[Serializable]
public struct TotalEnergy : IComponentData
{
    public float Value;
}
