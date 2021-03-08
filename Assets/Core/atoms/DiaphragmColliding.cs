using System;
using Unity.Entities;

/// <summary>
/// Marks an entity as an atom.
/// </summary>
[Serializable]
public struct DiaphragmColliding : IComponentData
{
    public bool Value;
}
