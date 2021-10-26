using System;
using Unity.Entities;

/// <summary>
/// Marks an atom as being in the upper path (initially excited state).
/// </summary>
[Serializable]
public struct Upper : IComponentData
{
    /// <summary>
    /// The entity taking the lower path.
    /// </summary>
    public Entity Lower;
}
