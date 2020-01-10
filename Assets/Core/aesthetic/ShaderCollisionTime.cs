using System;
using Unity.Entities;
using Unity.Rendering;

[Serializable]
[MaterialProperty("_CollTime", MaterialPropertyFormat.Float)]
public struct ShaderCollisionTime : IComponentData
{
    public float Value;
}