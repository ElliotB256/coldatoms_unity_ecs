using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Radius : IComponentData
{
    public float3 Value;
}
