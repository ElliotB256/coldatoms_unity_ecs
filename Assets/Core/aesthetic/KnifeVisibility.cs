using System;
using Unity.Entities;
using Unity.Rendering;

[Serializable]
[MaterialProperty("_Visible", MaterialPropertyFormat.Float)]
public struct KnifeVisibility : IComponentData
{
    public float Value;
}