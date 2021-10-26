using System;
using Unity.Entities;
using Unity.Rendering;

[Serializable]
[MaterialProperty("_Superposition", MaterialPropertyFormat.Float)]
public struct Superposition : IComponentData
{
    public float Value;
}