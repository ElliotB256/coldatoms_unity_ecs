using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


namespace Calculation {
[UpdateAfter(typeof(CalculateStats))]
[UpdateInGroup(typeof(CalculationSystemGroup))]
public class GatherStats : SystemBase
{
    float fpTemp = 0f;
    float ctTemp = 0f;
    float number = 0f;
    float Z0Number = 0f;
    float Z1Number = 0f;
    float Z2Number = 0f;

    float totalInternalEnergy = 0f;
    float Z0Energy = 0f;
    float Z1Energy = 0f;
    float Z2Energy = 0f;

    float T0 = 0f;
    float T1 = 0f;
    float T2 = 0f;

    float currentOsc = 0f;
    float maxOsc = 0f;

    float[] wallPressures = new float[20];

    const float kB = 1.3807e-16f;

    protected override void OnCreate()
    {
        // Enabled = false;  
    }

    protected override void OnUpdate()
    {
            // local values of total free path and collision time
            // Do I need to reset these on update?
        fpTemp = 0f;
        ctTemp = 0f;

        number = 0f;
        Z0Number = 0f;
        Z1Number = 0f;
        Z2Number = 0f;

        totalInternalEnergy = 0f;
        Z0Energy = 0f;
        Z1Energy = 0f;
        Z2Energy = 0f;
        
            // Gather statistical componetns 
        Entities.WithAll<Atom>().ForEach((in LastFreeTime lastFreeTime, in LastFreePath lastFreePath, in TotalEnergy totEnergy, in Zone zone) => {
            fpTemp += lastFreePath.Value;
            ctTemp += lastFreeTime.Value;
            totalInternalEnergy += totEnergy.Value;
            number += 1f;
            if (zone.Value == 0)
            {
                Z0Number += 1f;
                Z0Energy += totEnergy.Value;
            }
            else if (zone.Value == 1)
            {
                Z1Number += 1f;
                Z1Energy += totEnergy.Value;
            }
            else if (zone.Value == 2)
            {
                Z2Number += 1f;
                Z2Energy += totEnergy.Value;
            }

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


            // Gather oscillation data for compression runs
        Entities.WithAll<Piston>().ForEach((in Oscillations osc) => {
            currentOsc = osc.CurrentOscillation;
            maxOsc = osc.MaxOscillations;
        }).WithoutBurst().Run();
        

            // Set the component values
        Entities.WithAll<Statistics>().ForEach((ref MeanFreePath mfp, ref MeanCollisionTime mct, ref Number N, ref NZ0 nZ0, ref NZ1 nZ1, ref NZ2 nZ2) => {
            mfp.Value = fpTemp/number;
            mct.Value = ctTemp/number;
            
            N.Value = (int)number;
            nZ0.Value = (int)Z0Number;
            nZ1.Value = (int)Z1Number;
            nZ2.Value = (int)Z2Number;

        }).WithoutBurst().Run();

            // Another loop to set energy values
        Entities.WithAll<Statistics>().ForEach((ref TotalInternalEnergy U, ref UZ0 uZ0, ref UZ1 uZ1, ref UZ2 uZ2) => {
            U.Value = totalInternalEnergy;
            uZ0.Value = Z0Energy;
            uZ1.Value = Z1Energy;
            uZ2.Value = Z2Energy;

        }).WithoutBurst().Run();

        Entities.WithAll<Statistics>().ForEach((in NZ0 nZ0, in NZ1 nZ1, in NZ2 nZ2, in UZ0 uZ0, in UZ1 uZ1, in UZ2 uZ2) => {
            T0 = 2*uZ0.Value/(3*nZ0.Value*kB);
            T1 = 2*uZ1.Value/(3*nZ1.Value*kB);
            T2 = 2*uZ2.Value/(3*nZ2.Value*kB);
        }).WithoutBurst().Run();

        Entities.WithAll<Statistics>().ForEach((ref TZ0 tZ0, ref TZ1 tZ1, ref TZ2 tZ2) => {
            tZ0.Value = T0;
            tZ1.Value = T1;
            tZ2.Value = T2;
        }).WithoutBurst().Run();
        
        Entities.WithAll<Statistics>().ForEach((ref Oscillations osc) => {
            osc.CurrentOscillation = currentOsc;
            osc.MaxOscillations = maxOsc;
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
