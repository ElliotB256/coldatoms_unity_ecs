using System;
using Unity.Entities;

/// <summary>
/// Radius of entity used for scattering calculations.
/// </summary>
[Serializable]
public struct ScatteringRadius : IComponentData
{
    public float Value;
}
