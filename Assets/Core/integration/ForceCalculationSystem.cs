using Integration;
using Unity.Entities;

/// <summary>
/// A group for all force calculation systems
/// </summary>
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class ForceCalculationSystems : ComponentSystemGroup
{
}
