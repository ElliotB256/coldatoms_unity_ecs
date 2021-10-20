using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Calculation
{
    [DisallowMultipleComponent]
    public class HistogramProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public enum eQuantity
        {
            KineticEnergy,
            PotentialEnergy,
            TotalEnergy
        }

        public eQuantity Quantity;

        public Material Material;
        public float SamplingInterval = 0.1f;

        public float YAxisMin = 0f;
        public float YAxisMax = 1f;

        public float XAxisMin = 0f;
        public float XAxisMax = 1f;
        public int BinNumber = 30;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var x = dstManager.CreateEntity();
            var xData = dstManager.AddBuffer<DataPoint>(x);
            for (int i = 0; i < BinNumber; i++)
                xData.Add(
                    new DataPoint {
                        Value = XAxisMin + (XAxisMax - XAxisMin) / BinNumber * (i)
                        });
            dstManager.AddComponentData(x, new AxisLimit { Min = XAxisMin, Max = XAxisMax });

            var y = dstManager.CreateEntity();
            dstManager.AddBuffer<DataPoint>(y);

            switch (Quantity)
            {
                case eQuantity.KineticEnergy:
                    dstManager.AddComponentData(y, new KineticEnergyData());
                    break;
                case eQuantity.PotentialEnergy:
                    dstManager.AddComponentData(y, new PotentialEnergyData());
                    break;
                case eQuantity.TotalEnergy:
                    dstManager.AddComponentData(y, new TotalEnergyData());
                    break;
            }
            dstManager.AddComponentData(y, new AxisLimit { Min = YAxisMin, Max = YAxisMax });
            dstManager.AddComponentData(y, new Histogram { BinMin = XAxisMin, BinMax = XAxisMax, BinNumber = BinNumber });
            dstManager.AddComponentData(y, new SamplingInterval { Interval = SamplingInterval });

            dstManager.AddComponentData(entity, new Graph
            {
                X = x,
                Y = y,
                Length = BinNumber,
                LineWidth = 0.1f
            });

            dstManager.AddSharedComponentData(entity, new RenderMesh()
            {
                mesh = new Mesh(),
                castShadows = UnityEngine.Rendering.ShadowCastingMode.Off,
                material = Material,
                receiveShadows = false,
                subMesh = 0
            });
        }
    }
}