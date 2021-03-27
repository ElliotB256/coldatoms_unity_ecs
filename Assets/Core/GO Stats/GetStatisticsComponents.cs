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
    }

    private void LateUpdate()
    {
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
        UZ0 EU0 = manager.GetComponentData<UZ0>(statisticsEntity);
        UZ1 EU1 = manager.GetComponentData<UZ1>(statisticsEntity);
        UZ2 EU2 = manager.GetComponentData<UZ2>(statisticsEntity);

        Number EN = manager.GetComponentData<Number>(statisticsEntity);
        NZ0 EN0 = manager.GetComponentData<NZ0>(statisticsEntity);
        NZ1 EN1 = manager.GetComponentData<NZ1>(statisticsEntity);
        NZ2 EN2 = manager.GetComponentData<NZ2>(statisticsEntity);

        TotalVolume EV = manager.GetComponentData<TotalVolume>(statisticsEntity);
        Z0Volume EV0 = manager.GetComponentData<Z0Volume>(statisticsEntity);
        Z1Volume EV1 = manager.GetComponentData<Z1Volume>(statisticsEntity);
        Z2Volume EV2 = manager.GetComponentData<Z2Volume>(statisticsEntity);

        TZ0 ET0 = manager.GetComponentData<TZ0>(statisticsEntity);
        TZ1 ET1 = manager.GetComponentData<TZ1>(statisticsEntity);
        TZ2 ET2 = manager.GetComponentData<TZ2>(statisticsEntity);


        DynamicBuffer<BufferElementPressure> EPressure = manager.GetBuffer<BufferElementPressure>(statisticsEntity);
 
        // Here I update the Gameobject Component Data
        StatsScript.mfp = Emfp.Value;
        StatsScript.mct = Emct.Value;

        StatsScript.totIntEnergy = EtotIntEnergy.Value;
        StatsScript.U0 = EU0.Value;
        StatsScript.U1 = EU1.Value;
        StatsScript.U2 = EU2.Value;

        StatsScript.N = EN.Value;
        StatsScript.N0 = EN0.Value;
        StatsScript.N1 = EN1.Value;
        StatsScript.N2 = EN2.Value;

        StatsScript.V = EV.Value;
        StatsScript.V0 = EV0.Value;
        StatsScript.V1 = EV1.Value;
        StatsScript.V2 = EV2.Value;

        StatsScript.T0 = ET0.Value;
        StatsScript.T1 = ET1.Value;
        StatsScript.T2 = ET2.Value;

        StatsScript.currentTime = UnityEngine.Time.time;
        StatsScript.dT = UnityEngine.Time.deltaTime;

        for (int i = 0; i < EPressure.Length; i++)
        {
            StatsScript.pressures[i] = EPressure[i].Value;
        }
    }
}
