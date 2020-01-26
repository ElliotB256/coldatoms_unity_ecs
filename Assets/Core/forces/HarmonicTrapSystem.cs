using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Forces
{
    public struct HarmonicTrapCalculator : ICalculator<HarmonicTrap, Mass>
    {
        public float3 CalculateForce(in HarmonicTrap potential, in LocalToWorld potentialLocation, in LocalToWorld atomLocation, in Mass mass)
        {
            return potential.SpringConstant * (potentialLocation.Position - atomLocation.Position);
        }

        public float CalculatePotential(in HarmonicTrap potential, in LocalToWorld potentialLocation, in LocalToWorld atomLocation, in Mass mass)
        {
            return potential.SpringConstant * math.lengthsq(potentialLocation.Position - atomLocation.Position) / 2.0f;
        }
    }

    /// <summary>
    /// The harmonic trap system applies a force to all trapped atoms, for each harmonic trap.
    /// </summary>
    [UpdateInGroup(typeof(ForceCalculationSystems))]
    public class HarmonicTrapSystem : ExternalPotentialSystem<HarmonicTrap, Mass, HarmonicTrapCalculator>
    {
        protected override HarmonicTrapCalculator GetCalculator()
        {
            return new HarmonicTrapCalculator();
        }
    }

}