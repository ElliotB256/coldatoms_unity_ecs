using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct MeanCollisionTime : IComponentData
{
    public float Value;    
}
