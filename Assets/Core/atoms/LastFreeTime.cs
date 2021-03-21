using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct LastFreeTime : IComponentData
{
    public float Value;
}
