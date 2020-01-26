using System;
using Unity.Entities;
using Calculation;

/// <summary>
/// Kinetic energy associated with an entity.
/// </summary>
[Serializable]
public struct KineticEnergy : IComponentData, IAggregatable
{
    public float Value;

    public float GetValue()
    {
        return Value;
    }
}

/// <summary>
/// Potential Energy associated with an entity.
/// </summary>
[Serializable]
public struct PotentialEnergy : IComponentData, IAggregatable
{
    public float Value;

    public float GetValue()
    {
        return Value;
    }
}

[Serializable]
public struct TotalEnergy : IComponentData, IAggregatable
{
    public float Value;

    public float GetValue()
    {
        return Value;
    }
}
