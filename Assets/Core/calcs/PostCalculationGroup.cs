using Unity.Entities;

namespace Calculation { 
    
    [UpdateAfter(typeof(CalculationSystemGroup))]
    public class PostCalculationGroup : ComponentSystemGroup
    {
    }
}
