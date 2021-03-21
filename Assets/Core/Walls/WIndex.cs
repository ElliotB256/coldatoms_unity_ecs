using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct WIndex : IComponentData
{
    public int Value;    
}
