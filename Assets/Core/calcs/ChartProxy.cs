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

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var x = dstManager.CreateEntity();
            dstManager.AddBuffer<DataPoint>(x);
            dstManager.AddComponentData(x, new TimeData());
            dstManager.AddComponentData(x, new DataLength { Value = SeriesLength });
            dstManager.AddComponentData(x, new DataRange { Min = 0f, Max = 10f });

            var y = dstManager.CreateEntity();
            dstManager.AddBuffer<DataPoint>(y);
            dstManager.AddComponentData(y, new TimeData());
            dstManager.AddComponentData(y, new DataLength { Value = SeriesLength });
            dstManager.AddComponentData(y, new DataRange { Min = 0f, Max = 10f });

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