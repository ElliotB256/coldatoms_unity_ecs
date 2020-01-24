using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Calculation
{
    [UpdateInGroup(typeof(CalculationSystemGroup))]
    public class KineticEnergySystem : JobComponentSystem
    {
        private EntityQuery AtomQuery;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            int AtomNumber = AtomQuery.CalculateEntityCount();
            NativeArray<float> KineticEnergies = new NativeArray<float>(AtomNumber, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            inputDeps.Complete();

            var CalculateKE = Entities
                .WithAll<Atom,Trapped>()
                .WithStoreEntityQueryInField(ref AtomQuery)
                .ForEach(
                    (int entityInQueryIndex, in Velocity velocity, in Mass mass) =>
                        KineticEnergies[entityInQueryIndex] = 0.5f * mass.Value * math.dot(velocity.Value, velocity.Value)
                ).Schedule(inputDeps);
            

            NativeArray<float> TotalKE = new NativeArray<float>(1, Allocator.TempJob);
            var GetTotalKE = Job.WithCode(
                () =>
                {
                    for (int i = 0; i < AtomNumber; i++)
                        TotalKE[0] = TotalKE[0] + KineticEnergies[i];
                }
                )
                .WithDeallocateOnJobCompletion(KineticEnergies)
                .Schedule(CalculateKE);

            var UpdateTotalKE = Entities
                .WithAll<TotalKineticEnergyData>()
                .WithNativeDisableContainerSafetyRestriction(TotalKE)
                .ForEach((ref CurrentDataValue data) => data.Value = TotalKE[0])
                .Schedule(GetTotalKE);

            var UpdateAverageKE = Entities
                .WithAll<AverageKineticEnergyData>()
                .WithNativeDisableContainerSafetyRestriction(TotalKE)
                .ForEach((ref CurrentDataValue data) => data.Value = TotalKE[0] / AtomNumber)
                .WithDeallocateOnJobCompletion(TotalKE)
                .Schedule(UpdateTotalKE);

            return UpdateAverageKE;

        }
    }

    public struct TotalKineticEnergyData : IComponentData { }
    public struct AverageKineticEnergyData : IComponentData { }
}