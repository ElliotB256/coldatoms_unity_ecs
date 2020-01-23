﻿using Unity.Entities;

/// <summary>
/// A data point that can be plotted.
/// </summary>
public struct DataPoint : IBufferElementData
{
    public float Value;
}

public struct DataLength : IComponentData
{
    public int Value;
}
