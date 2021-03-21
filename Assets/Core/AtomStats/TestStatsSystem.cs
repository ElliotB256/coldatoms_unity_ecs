using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Calculation {
public class TestStatsSystem : SystemBase
{
    protected override void OnUpdate()
    {        
        Entities.WithAll<Statistics>().ForEach((ref MeanFreePath mfp) => {

        }).Schedule();
    }
}
}