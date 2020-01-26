using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Calculation
{
    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class ChartProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Material Material;
        public int SeriesLength = 100;
        public float SamplingInterval = 0.1f;

        public float YAxisMin = 0f;
        public float YAxisMax = 1f;

        public float XAxisMin = 0f;
        public float XAxisMax = 1f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var x = dstManager.CreateEntity();
            dstManager.AddBuffer<DataPoint>(x);
            dstManager.AddComponentData(x, new TimeData());
            dstManager.AddComponentData(x, new DataLength { Value = SeriesLength });
            dstManager.AddComponentData(x, new AxisLimit { Min = XAxisMin, Max = XAxisMax });
            dstManager.AddComponentData(x, new CurrentDataValue { Value = 0f });
            dstManager.AddComponentData(x, new SamplingInterval { Interval = SamplingInterval });

            var y = dstManager.CreateEntity();
            dstManager.AddBuffer<DataPoint>(y);
            dstManager.AddComponentData(y, new KineticEnergyData());
            dstManager.AddComponentData(y, new Average());
            dstManager.AddComponentData(y, new DataLength { Value = SeriesLength });
            dstManager.AddComponentData(y, new AxisLimit { Min = YAxisMin, Max = YAxisMax });
            dstManager.AddComponentData(y, new CurrentDataValue { Value = 0f });
            dstManager.AddComponentData(y, new SamplingInterval { Interval = SamplingInterval });

            dstManager.AddComponentData(entity, new Graph
            {
                X = x,
                Y = y,
                Length = SeriesLength,
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

    public struct Graph : IComponentData
    {
        public Entity X;
        public Entity Y;
        public int Length;
        public float LineWidth;
    }
}