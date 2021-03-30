using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Oscillations : IComponentData
{
    public float CurrentOscillation;
    public float MaxOscillations;
}
