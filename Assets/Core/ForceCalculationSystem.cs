using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

/// <summary>
/// A group for all force calculation systems
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class ForceCalculationSystems : ComponentSystemGroup
{
}
