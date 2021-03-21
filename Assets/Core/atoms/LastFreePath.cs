using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct LastFreePath : IComponentData
{
    public float Value;
}
