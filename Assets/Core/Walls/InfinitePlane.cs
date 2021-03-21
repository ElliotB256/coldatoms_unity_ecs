using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Represents a triangular segment of wall that atoms can deflect off.
/// </summary>
[Serializable]
public struct InfinitePlane : IComponentData
{
    public float3 V1;
    public float3 Normal;
}
