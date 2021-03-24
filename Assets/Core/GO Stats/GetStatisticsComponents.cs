using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;


// This script grabs the Entity data and pushes it to another component on the gameobject 
public class GetStatisticsComponents : MonoBehaviour
{
        // Load in the script that will hold the values;
    public Stats StatsScript;

    public Entity statisticsEntity = Entity.Null;
    public EntityManager manager;
    public EntityQuery StatisticsQuery;
    EntityQueryDesc StatisticsQueryDesc;

    private void Awake()
    {
        Stats StatsScript = GetComponent<Stats>();
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        StatisticsQueryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<Statistics>()
            }
        };
        
        // StatisticsQuery = manager.CreateEntityQuery(StatisticsQueryDesc);
        // statisticsEntity = StatisticsQuery.GetSingletonEntity();
        // Debug.Log(manager.GetComponentData<MeanFreePath>(statisticsEntity).Value);
    }

    private void LateUpdate()
    {

        // Debug.Log(manager.GetComponentData<MeanFreePath>(statisticsEntity).Value);

        // StatsScript.mfp += 1f;
        // Debug.Log(StatsScript.mfp);

            // This logic may cause the first frame of data to be dropped
        if (statisticsEntity == Entity.Null) 
        {
            StatisticsQuery = manager.CreateEntityQuery(StatisticsQueryDesc);
            statisticsEntity = StatisticsQuery.GetSingletonEntity();

            PullPushData();
            

        } else 
        {
            PullPushData();
            
        }
    }


    void PullPushData() {
        // Here I get the Entity Component Data
        MeanFreePath Emfp = manager.GetComponentData<MeanFreePath>(statisticsEntity);
        MeanCollisionTime Emct = manager.GetComponentData<MeanCollisionTime>(statisticsEntity);
        TotalInternalEnergy EtotIntEnergy = manager.GetComponentData<TotalInternalEnergy>(statisticsEntity);
        Number EN = manager.GetComponentData<Number>(statisticsEntity);
        DynamicBuffer<BufferElementPressure> EPressure = manager.GetBuffer<BufferElementPressure>(statisticsEntity);
 
        // Here I update the Gameobject Component Data
        StatsScript.mfp = Emfp.Value;
        StatsScript.mct = Emct.Value;
        StatsScript.totIntEnergy = EtotIntEnergy.Value;
        StatsScript.N = EN.Value;
        for (int i = 0; i < EPressure.Length; i++)
        {
            StatsScript.pressures[i] = EPressure[i].Value;
        }
    }
}
