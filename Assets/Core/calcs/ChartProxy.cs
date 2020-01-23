using Unity.Entities;
using UnityEngine;

namespace Calculation
{
    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class ChartProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int SeriesLength = 10;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new KineticEnergyData());
            dstManager.AddBuffer<DataPoint>(entity);
            dstManager.AddComponentData(entity, new DataLength { Value = SeriesLength });
        }
    }
}