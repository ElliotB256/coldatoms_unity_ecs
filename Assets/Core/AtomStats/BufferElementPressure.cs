using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[InternalBufferCapacity(20)]
public struct BufferElementPressure : IBufferElementData
{
    public float Value;
}
