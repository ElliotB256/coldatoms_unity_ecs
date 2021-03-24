using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


namespace Calculation {
[UpdateAfter(typeof(CalculateStats))]
[UpdateInGroup(typeof(CalculationSystemGroup))]
public class GatherStats : SystemBase
{
    float fpTemp = 0f;
    float ctTemp = 0f;
    float number = 0f;
    float totalInternalEnergy = 0f;
    float[] wallPressures = new float[20];

    protected override void OnCreate()
    {
        // Enabled = true;
    }

    protected override void OnUpdate()
    {
            // local values of total free path and collision time
            // Do I need to reset these on update?
        fpTemp = 0f;
        ctTemp = 0f;
        number = 0f;
        totalInternalEnergy = 0f;

            // Gather statistical componetns 
        Entities.WithAll<Atom>().ForEach((in LastFreeTime lastFreeTime, in LastFreePath lastFreePath, in TotalEnergy totEnergy) => {
            fpTemp += lastFreePath.Value;
            ctTemp += lastFreeTime.Value;
            totalInternalEnergy += totEnergy.Value;
            number += 1f;
        }).WithoutBurst().Run();

            // limit of 6 inputs
                // This covers the static walls
        Entities.ForEach((in Pressure pZ0, in PressureZ1 pZ1, in PressureZ2 pZ2, in WIndex wi0, in WZ1Index wi1, in WZ2Index wi2) => {
            wallPressures[wi0.Value] = pZ0.Value;
            if (wi1.Value != -1) {
                wallPressures[wi1.Value] = pZ1.Value;
            }
            if (wi2.Value != -1) {
                wallPressures[wi2.Value] = pZ2.Value;
            }
        }).WithoutBurst().Run();

            // This covers the Piston and the Diaphragm
        Entities.ForEach((in Pressure pR, in PressureLeft pL, in WIndex wiR, in WIndexLeft wiL) => {
            wallPressures[wiR.Value] = pR.Value;
            wallPressures[wiL.Value] = pL.Value;
        }).WithoutBurst().Run();


        
            // Set the component values
        Entities.WithAll<Statistics>().ForEach((ref MeanFreePath mfp, ref MeanCollisionTime mct, ref TotalInternalEnergy TotalInternalEnergy, ref Number N) => {
            mfp.Value = fpTemp/number;
            mct.Value = ctTemp/number;
            TotalInternalEnergy.Value = totalInternalEnergy;
            N.Value = (int)number;
        }).WithoutBurst().Run();



        Entities.WithAll<Statistics>().ForEach((DynamicBuffer<BufferElementPressure> dynamicBuffer) => {
            for (int i = 0; i < dynamicBuffer.Length; i ++)
            {
                BufferElementPressure arrayElement = dynamicBuffer[i];
                arrayElement.Value = wallPressures[i];
                dynamicBuffer[i] = arrayElement;
            }
        }).WithoutBurst().Run();
    }
}
}
