using Unity.Entities;

/// <summary>
/// A data point that can be plotted.
/// </summary>
public struct DataPoint : IBufferElementData
{
    public float Value;
}

/// <summary>
/// Desired length of a Data point buffer
/// </summary>
public struct DataLength : IComponentData
{
    public int Value;
}

/// <summary>
/// Data range over which to plot.
/// </summary>
public struct AxisLimit : IComponentData
{
    public float Min;
    public float Max;
}

/// <summary>
/// Current value of the data point.
/// </summary>
public struct CurrentDataValue : IComponentData
{
    public float Value;
}

/// <summary>
/// Interval at which data should be sampled.
/// </summary>
public struct SamplingInterval : IComponentData
{
    public float Interval;
    public float Remaining;
}