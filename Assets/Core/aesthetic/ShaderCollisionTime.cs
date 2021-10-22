using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

[Serializable]
[MaterialProperty("_CollisionTime", MaterialPropertyFormat.Float)]
public struct ShaderCollisionTime : IComponentData
{
    public float Value;
}