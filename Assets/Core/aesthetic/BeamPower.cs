using System;
using Unity.Entities;
using Unity.Rendering;

[GenerateAuthoringComponent]
[Serializable]
[MaterialProperty("_Power", MaterialPropertyFormat.Float)]
public struct BeamPower : IComponentData
{
    public float Value;
}