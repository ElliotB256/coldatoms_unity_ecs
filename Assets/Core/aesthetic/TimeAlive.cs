using System;
using Unity.Entities;
using Unity.Rendering;

[GenerateAuthoringComponent]
[Serializable]
[MaterialProperty("_TimeAlive", MaterialPropertyFormat.Float)]
public struct TimeAlive : IComponentData
{
    public float Value;
}