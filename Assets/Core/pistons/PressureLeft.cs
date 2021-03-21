using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct PressureLeft : IComponentData
{
    public float Value;
}
