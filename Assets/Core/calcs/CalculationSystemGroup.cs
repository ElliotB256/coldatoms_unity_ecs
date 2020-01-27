using Integration;
using Unity.Entities;

namespace Calculation { 
    /// <summary>
    /// System in which calculations of quantities can occur.
    /// </summary>
    [UpdateInGroup(typeof(FixedUpdateGroup))]
    public class CalculationSystemGroup : ComponentSystemGroup
    {
    }
}
