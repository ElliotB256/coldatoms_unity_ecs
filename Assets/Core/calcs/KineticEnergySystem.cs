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
            NativeArray<float> KineticEnergies = new NativeArray<float>(AtomQuery.CalculateEntityCount(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var CalculateKE = Entities
                .WithAll<Atom>()
                .WithStoreEntityQueryInField(ref AtomQuery)
                .ForEach(
                    (int entityInQueryIndex, in Velocity velocity, in Mass mass) =>
                        KineticEnergies[entityInQueryIndex] = 0.5f * mass.Value * math.dot(velocity.Value, velocity.Value)
                ).Schedule(inputDeps);

            NativeArray<float> TotalKE = new NativeArray<float>(1, Allocator.TempJob);
            var GetTotalKE = Job.WithCode(
                () =>
                {
                    for (int i = 0; i < KineticEnergies.Length; i++)
                        TotalKE[0] = TotalKE[0] + KineticEnergies[i];
                }
                )
                .WithDeallocateOnJobCompletion(KineticEnergies)
                .Schedule(CalculateKE);


            float time = (float)Time.ElapsedTime;
            return Entities
                .WithAll<KineticEnergyData>()
                .ForEach(
                    (DynamicBuffer<DataPoint> data) => data.Add(new DataPoint { Value = TotalKE[0] })
                )
                .WithDeallocateOnJobCompletion(TotalKE)
                .Schedule(GetTotalKE);
        }
    }

    public struct KineticEnergyData : IComponentData { }
}