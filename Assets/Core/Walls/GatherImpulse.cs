using Integration;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;


[UpdateInGroup(typeof(FixedUpdateGroup))]
[UpdateAfter(typeof(DiaphragmCollisionSystem))]
public class GatherImpulse : SystemBase
{
    // Indices: 
    //  0 Z0 x-,  1 Z0 y+,  2 Z0 x+,  3 Z0 y-, 4 Z0 z+, 5 Z0 z-, 6 Piston right, 7 Piston left, 8 Diaphragm right, 9 Diaphragm left, 
    // 10 Z1 y+, 11 Z1 y-, 12 Z1 z+, 13 Z1 z-, 
    // 14 Z2 y+, 15 Z2 y-, 16 Z2 z+, 17 Z2 z-
    // 18 Z1 x+, 19 Z2 Z+

    
    
    public float dT;
    public float L = 20;
    float xP;
    float xD;
    public float V0 = 0;
    public float V1 = 0;
    public float V2 = 0;

    public EntityManager manager;

    EntityQuery PistonQuery;
    EntityQuery DiaphragmQuery;

    public Entity pistonEntity = Entity.Null;
    public Entity diaphragmEntity = Entity.Null;

    Translation pistonTranslation;
    Translation diaphragmTranslation;
    
    protected override void OnCreate()
    {
        // Enabled = true;
        
        PistonQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<Piston>(),
                ComponentType.ReadOnly<Translation>()
            }
        });

        DiaphragmQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<Diaphragm>(),
                ComponentType.ReadOnly<Translation>()
            }
        });

        manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    

    protected override void OnUpdate()
    {
        NativeArray<float> totalImpulse = new NativeArray<float>(20, Allocator.TempJob);

        bool PistonExists = HasSingleton<Piston>();
        bool DiaphragmExists = HasSingleton<Diaphragm>();

        if (PistonExists)
        {
            pistonEntity = PistonQuery.GetSingletonEntity();
            pistonTranslation = manager.GetComponentData<Translation>(pistonEntity);
            xP = pistonTranslation.Value.x;
        }
    
        if (DiaphragmExists)
        {
            diaphragmEntity = DiaphragmQuery.GetSingletonEntity();
            diaphragmTranslation = manager.GetComponentData<Translation>(diaphragmEntity);
            xD = diaphragmTranslation.Value.x;
        }


        dT = UnityEngine.Time.deltaTime;

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


        // First deal with pistons and diphragms

            // Loop for the Piston and Diaphragm, left side of wall pressures
        Entities.ForEach((ref PressureLeft pressure, in WIndexLeft wallIndex) => {
            pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*L);
        }).WithoutBurst().Run();
        


            // Both piston and Diaphragm exist
        if (PistonExists && DiaphragmExists) //PistonNumber == 1 && DiaphragmNumber == 1)//manager.Exists(pistonEntity) && manager.Exists(diaphragmEntity))
        {
            float z0Dim = Mathf.Abs(-L/2 - xP);
            float z1Dim = Mathf.Abs(xD - xP);
            float z2Dim = Mathf.Abs(L/2 - xD);

            V0 = L*L*z0Dim;
            V1 = L*L*z1Dim;
            V2 = L*L*z2Dim;

                // Z0 Surfaces
            Entities.ForEach((ref Pressure pressure, in WIndex wallIndex) => {
                    // Z0 surfaces y and z surfaces
                if (wallIndex.Value == 1 || wallIndex.Value == 3 || wallIndex.Value == 4 || wallIndex.Value == 5)
                {
                    pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*z0Dim);
                }
                    // other surfaces
                else 
                {
                    pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*L);
                }
            }).WithoutBurst().Run();

                // Z1 Surfaces
            Entities.ForEach((ref PressureZ1 pressure, in WZ1Index wallIndex) => {
                    // Z1 y and z surfaces
                if (wallIndex.Value == 10 || wallIndex.Value == 11 || wallIndex.Value == 12 || wallIndex.Value == 13)
                {
                    pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*z1Dim);
                }
                    // Other Z1 surfaces
                else if (wallIndex.Value >= 0)
                {
                    pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*L);
                }
            }).WithoutBurst().Run();

                // Z2 Surfaces
            Entities.ForEach((ref PressureZ2 pressure, in WZ2Index wallIndex) => {
                    // Z2 y and z surfaces
                if (wallIndex.Value == 14 || wallIndex.Value == 15 || wallIndex.Value == 16 || wallIndex.Value == 17)
                {
                    pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*z2Dim);
                }
                    // Other Z2 surfaces
                else if (wallIndex.Value >= 0)
                {
                    pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*L);
                }
            }).WithoutBurst().Run();

        }


            // Only piston
        else if (PistonExists && !DiaphragmExists) //PistonNumber == 1 && DiaphragmNumber == 0) //manager.Exists(pistonEntity) && !(manager.Exists(diaphragmEntity)))
        {
            float z0Dim = Mathf.Abs(-L/2 - xP);
            float z1Dim = Mathf.Abs(L/2 - xP);

            V0 = L*L*z0Dim;
            V1 = L*L*z1Dim;

                // Z0 Surfaces
            Entities.ForEach((ref Pressure pressure, in WIndex wallIndex) => {
                    // Z0 surfaces y and z surfaces
                if (wallIndex.Value == 1 || wallIndex.Value == 3 || wallIndex.Value == 4 || wallIndex.Value == 5)
                {
                    pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*z0Dim);
                }
                    // other surfaces
                else 
                {
                    pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*L);
                }
            }).WithoutBurst().Run();

                // Z1 Surfaces
            Entities.ForEach((ref PressureZ1 pressure, in WZ1Index wallIndex) => {
                    // Z1 y and z surfaces
                if (wallIndex.Value == 10 || wallIndex.Value == 11 || wallIndex.Value == 12 || wallIndex.Value == 13)
                {
                    pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*z1Dim);
                }
                    // Other Z1 surfaces
                else if (wallIndex.Value >= 0)
                {
                    pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*L);
                }
            }).WithoutBurst().Run();
        }


            // Only Diaphragm Exists 
        else if (!PistonExists && DiaphragmExists) //PistonNumber == 0 && DiaphragmNumber == 1) //!(manager.Exists(pistonEntity)) && manager.Exists(diaphragmEntity))
        {
                // Z0 and Z1 exist
            float z0Dim = Mathf.Abs(-L/2 - xD);
            float z1Dim = Mathf.Abs(L/2 - xD);

            V0 = L*L*z0Dim;
            V1 = L*L*z1Dim;
            
                // Z0 Surfaces
            Entities.ForEach((ref Pressure pressure, in WIndex wallIndex) => {
                    // Z0 surfaces y and z surfaces
                if (wallIndex.Value == 1 || wallIndex.Value == 3 || wallIndex.Value == 4 || wallIndex.Value == 5)
                {
                    pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*z0Dim);
                }
                    // other surfaces
                else 
                {
                    pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*L);
                }
            }).WithoutBurst().Run();

                // Z1 Surfaces
            Entities.ForEach((ref PressureZ1 pressure, in WZ1Index wallIndex) => {
                    // Z1 y and z surfaces
                if (wallIndex.Value == 10 || wallIndex.Value == 11 || wallIndex.Value == 12 || wallIndex.Value == 13)
                {
                    pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*z1Dim);
                }
                    // Other Z1 surfaces
                else if (wallIndex.Value >= 0)
                {
                    pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*L);
                }
            }).WithoutBurst().Run();
        }


            // No Diaphragm or Piston
        else if (!PistonExists && !DiaphragmExists) //PistonNumber == 0 && DiaphragmNumber == 0) //!(manager.Exists(pistonEntity)) && !(manager.Exists(diaphragmEntity)))
        {
                // only Z0 exists 
            float z0Dim = L;

            V0 = L*L*z0Dim;
            
                    // This loop is over all Z0 6 walls and the right side of the piston and diaphragm
            Entities.ForEach((ref Pressure pressure, in WIndex wallIndex) => {
                pressure.Value = totalImpulse[wallIndex.Value]/(dT*L*z0Dim);
            }).WithoutBurst().Run();   
        }

        Entities.WithAll<Statistics>().ForEach((ref Z0Volume VZ0, ref Z1Volume VZ1, ref Z2Volume VZ2, ref TotalVolume V) => {
            VZ0.Value = V0;
            VZ1.Value = V1;
            VZ2.Value = V2;
            V.Value = V0 + V1 + V2;
        }).WithoutBurst().Run();

        totalImpulse.Dispose();

        //     // This loop is over all 6 walls and the right side of the piston and diaphragm
        // Entities.ForEach((ref Pressure pressure, in WIndex wallIndex) => {
        //     // Calculate the pressure on each wall based on the total impulse
        //     pressure.Value = totalImpulse[wallIndex.Value]/dT;
        // }).WithoutBurst().Run();

        // Entities.ForEach((ref PressureZ1 pressure, in WZ1Index wallIndex) => {
        //     if (wallIndex.Value >= 0) {
        //         pressure.Value = totalImpulse[wallIndex.Value]/dT;
        //     }
        // }).WithoutBurst().Run();

        // Entities.ForEach((ref PressureZ2 pressure, in WZ2Index wallIndex) => {
        //     if (wallIndex.Value >= 0) {
        //         pressure.Value = totalImpulse[wallIndex.Value]/dT;
        //     }
        // }).WithoutBurst().Run();
    }
}
