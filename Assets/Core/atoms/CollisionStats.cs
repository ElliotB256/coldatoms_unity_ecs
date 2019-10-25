using System;
using Unity.Entities;

[Serializable]
public struct CollisionStats : IComponentData
{
    /// <summary>
    /// Time since last collision, in seconds
    /// </summary>
    public float TimeSinceLastCollision;
}
