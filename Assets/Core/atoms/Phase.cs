using System;
using Unity.Entities;

/// <summary>
/// Interferometric phase picked up by a path.
/// </summary>
[Serializable]
public struct Phase : IComponentData
{
    public float Value;
}
