using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Calculation
{
    public class EnergyHistogramSystem : HistogramSystem<TotalEnergyData, TotalEnergy> { }

    [UpdateInGroup(typeof(CalculationSystemGroup))]
    public class HistogramSystem<TBinner,TComponent> : JobComponentSystem
        where TBinner : struct, IComponentData
        where TComponent : struct, IComponentData, IAggregatable
    {
        EntityQuery HistogramQuery;
        EntityQuery AtomQuery;

        protected override void OnCreate()
        {
            HistogramQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<Histogram>(),
                    ComponentType.ReadOnly<TBinner>(),
                    ComponentType.ReadWrite<DataPoint>()
                }
            });

            AtomQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {
                    ComponentType.ReadOnly<Atom>(),
                    ComponentType.ReadOnly<TComponent>(),
                    ComponentType.ReadOnly<Trapped>(),
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // What's the best way to sort this? 
            // Probably loop over atoms, sort atoms into each histogram box.
            // But how to do this quickly for arbitrary number of histograms with unique ranges?
            // Given nuber of histograms will be low, could actually create a job to sort each histogram.
            //
            // alternative would be to store offsets and bin values for each histogram explicitly - which is a bit more of a pain
            // because we don't know the sizes of the histograms.


            int histogramNumber = HistogramQuery.CalculateEntityCount();
            var counts = new NativeArray<int>(histogramNumber * Histogram.MAX_BIN_NUMBER, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            var components = AtomQuery.ToComponentDataArray<TComponent>(Allocator.TempJob);

            
            var getCount = new GetCountsJob
            {
                Counts = counts,
                Components = components
            }.Schedule(HistogramQuery, inputDeps);

            var assignCounts = new AssignCountsJob
            {
                Counts = counts
            }.Schedule(HistogramQuery, getCount);

            //counts.Dispose(assignCounts);
            //components.Dispose(assignCounts);

            return assignCounts;
        }

        struct GetCountsJob : IJobForEachWithEntity<Histogram>
        {
            [NativeDisableParallelForRestriction] public NativeArray<int> Counts;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<TComponent> Components;

            public void Execute(Entity entity, int index, [ReadOnly] ref Histogram histogram)
            {
                int offset = index * Histogram.MAX_BIN_NUMBER;
                for (int i = 0; i < Components.Length; i++)
                {
                    int binId = histogram.GetBinID(Components[i].GetValue());
                    if (binId < 0 || binId >= histogram.BinNumber)
                        continue;
                    Counts[binId + offset]++;
                }
            }
        }

        struct AssignCountsJob : IJobForEachWithEntity_EBC<DataPoint, Histogram>
        {
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> Counts;

            public void Execute(Entity entity, int index, DynamicBuffer<DataPoint> buffer, [ReadOnly] ref Histogram histogram)
            {
                int offset = histogram.GetOffset(index);
                buffer.Clear();
                for (int i = 0; i < histogram.BinNumber; i++)
                    buffer.Add(new DataPoint { Value = Counts[offset + i] });
            }
        }
    }

    /// <summary>
    /// Represents a histogram.
    /// </summary>
    public struct Histogram : IComponentData {
        public float BinMin;
        public float BinMax;
        public int BinNumber;

        /// <summary>
        /// Maximum number of bins in a single histogram.
        /// </summary>
        public const int MAX_BIN_NUMBER = 256;

        public int GetBinID(float value)
        {
            return (int)(BinNumber * (value - BinMin) / (BinMax - BinMin));
        }

        public int GetOffset(int id)
        {
            return id * MAX_BIN_NUMBER;
        }
    }
}