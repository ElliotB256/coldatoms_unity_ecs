using System;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Previous frame's value of Force
/// </summary>
[Serializable]
public struct PrevForce : IComponentData
{
    public float3 Value;
}
