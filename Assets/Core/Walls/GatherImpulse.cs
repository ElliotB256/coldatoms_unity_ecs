using Integration;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;


[UpdateInGroup(typeof(FixedUpdateGroup))]
[UpdateAfter(typeof(DiaphragmCollisionSystem))]
public class GatherImpulse : SystemBase
{
    // Indices: 
    //  0 Z0 x-,  1 Z0 y+,  2 Z0 x+,  3 Z0 y-, 4 Z0 z+, 5 Z0 z-, 6 Piston right, 7 Piston left, 8 Diaphragm right, 9 Diaphragm left, 
    // 10 Z1 y+, 11 Z1 y-, 12 Z1 z+, 13 Z1 z-, 
    // 14 Z2 y+, 15 Z2 y-, 16 Z2 z+, 17 Z2 z-
    // 18 Z1 x+, 19 Z2 Z+
    [DeallocateOnJobCompletion] public NativeArray<float> totalImpulse = new NativeArray<float>(20, Allocator.TempJob);
    protected override void OnCreate()
    {
        // Enabled = true;
        
        // totalImpulse = new NativeArray<float>(6, Allocator.TempJob);
    }
    

    protected override void OnUpdate()
    {
            // Is there an easier way to reset the array?
        for (int i = 0; i < totalImpulse.Length; i ++)
        {
            totalImpulse[i] = 0;
        }

            // Gather impulse of each atom into the totalImpulse Array
        Entities.WithAll<Atom>().ForEach((ref WallCollisions wallCollisions, in Zone zone) => {

            if (wallCollisions.Impulse > 0 && wallCollisions.WallIndex >= 0) {
                totalImpulse[wallCollisions.WallIndex] += wallCollisions.Impulse;
            }

                // Set the impulse of each atom to 0
                // What should the index be? I don't need to reset it because this won't affect the logic but is null a valid value?
            wallCollisions.Impulse = 0f;
        }).WithoutBurst().Run();

            // This loop is over all 6 walls and the right side of the piston and diaphragm
        Entities.ForEach((ref Pressure pressure, in WIndex wallindex) => {
            // Calculate the pressure on each wall based on the total impulse
                // Divide this by the area 400 = 20x20?
            pressure.Value = totalImpulse[wallindex.Value];
        }).WithoutBurst().Run();

        Entities.ForEach((ref PressureZ1 pressure, in WZ1Index wallIndex) => {
            if (wallIndex.Value >= 0) {
                pressure.Value = totalImpulse[wallIndex.Value];
            }
        }).WithoutBurst().Run();

        Entities.ForEach((ref PressureZ2 pressure, in WZ2Index wallIndex) => {
            if (wallIndex.Value >= 0) {
                pressure.Value = totalImpulse[wallIndex.Value];
            }
        }).WithoutBurst().Run();

            // Loop for the Piston and Diaphragm, left side of wall pressures
        Entities.ForEach((ref PressureLeft pressure, in WIndexLeft wallIndex) => {
            pressure.Value = totalImpulse[wallIndex.Value];
        }).WithoutBurst().Run();


    }
}
