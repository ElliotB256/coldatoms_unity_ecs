using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct TotalInternalEnergy : IComponentData
{ 
    public float Value;
}
