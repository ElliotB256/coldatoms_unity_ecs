using System;
using Unity.Entities;

/// <summary>
/// Radius of entity used for collision calculations.
/// </summary>
[Serializable]
public struct CollisionRadius : IComponentData
{
    public float Value;
}
