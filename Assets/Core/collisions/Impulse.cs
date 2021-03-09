using System;
using Unity.Entities;

/// <summary>
/// Impulse recieved each frame from colliding particles
/// </summary>
[Serializable]
public struct Impulse : IComponentData
{
    public float Value;
}
