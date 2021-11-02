﻿using Integration;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(FixedUpdateGroup))]
public class MakeMeasurementSystem : SystemBase
{
    protected override void OnCreate()
    {
        lowerQ = GetEntityQuery(ComponentType.ReadOnly<Atom>(), ComponentType.Exclude<Upper>());
        upperQ = GetEntityQuery(ComponentType.ReadOnly<Atom>(), ComponentType.ReadOnly<Upper>());
    }

    EntityQuery upperQ;
    EntityQuery lowerQ;

    protected override void OnUpdate()
    {
        var sequence = GetSingleton<Sequence>();

        if (sequence.Stage != SequenceStage.MakeMeasurement)
            return;

        var upper = (float)upperQ.CalculateEntityCount();
        var lower = (float)lowerQ.CalculateEntityCount();

        var reading = (upper - lower) / (upper + lower) * 2f * 10f;

        float error = 60f / math.pow(((float)upper + (float)lower), 0.5f);

        // OK, major bluff incoming. The truth is that a hotter cloud is better for the simulation, because the simulation
        // doesn't have many of the real imperfections like shear, LMT, optical density etc that favor cold clouds.
        // To get the idea across that colder is better we instead use a MeasurementRegionWidth and only count the atoms within that radius.

        // Probably need to use run to perform the count.
        var count = 0;
        var avgR2 = 0f;
        var number = 0;

        Entities.WithAll<Atom>().ForEach((in Translation t) => {
            var r2 = t.Value.x * t.Value.x + t.Value.z * t.Value.z;
            avgR2 += r2;
            if (r2 < sequence.MeasurementRegionWidth)
                count++;
            number++;
        }
        ).Run();

        avgR2 /= (float)number;
        Debug.Log(string.Format("avgR2: {0}", avgR2));
        
        // error using number: favors hot clouds.
        error = 60f / math.pow(count, 0.5f);

        // fudged error from avgR2 and number.
        float score = math.exp(-avgR2 / 7f) * number;
        error = 60f / math.pow(score, 0.5f);
        error = math.clamp(error, 0f, 55f);

        //add a random noise to the reading. (uniform, but w/e)
        reading = reading + UnityEngine.Random.Range(-error, error) / 3f;

        var x = 20f + sequence.StartingTime / 6f * 6f;
        var y = 40f + reading;
        var width = 0.25f;

        // Error bar
        var bar = EntityManager.Instantiate(sequence.GraphPointTemplate);
        EntityManager.SetComponentData(bar, new Translation { Value = new float3 { x = x, y = y, z = 0f } });
        EntityManager.AddComponentData(bar, new NonUniformScale { Value = new float3 { x = width, y = error, z = 0.5f } });

        // Upper cap
        var upCap = EntityManager.Instantiate(sequence.GraphPointTemplate);
        EntityManager.SetComponentData(upCap, new Translation { Value = new float3 { x = x, y = y + error / 2f, z = 0f } });
        EntityManager.AddComponentData(upCap, new NonUniformScale { Value = new float3 { x = 1.3f, y = width, z = 0.5f } });

        // Lower cap
        var lowCap = EntityManager.Instantiate(sequence.GraphPointTemplate);
        EntityManager.SetComponentData(lowCap, new Translation { Value = new float3 { x = x, y = y - error / 2f, z = 0f } });
        EntityManager.AddComponentData(lowCap, new NonUniformScale { Value = new float3 { x = 1.3f, y = width, z = 0.5f } });

        // Point
        var point = EntityManager.Instantiate(sequence.GraphCentrePointTemplate);
        EntityManager.SetComponentData(point, new Translation { Value = new float3 { x = x, y = y, z = 0f } });
        EntityManager.AddComponentData(point, new NonUniformScale { Value = new float3 { x = 0.6f, y = 0.6f, z = 1f } });
        EntityManager.AddComponentData(point, new Rotation { Value = quaternion.RotateZ(math.PI * 45f / 180f) });

        sequence.Stage++;
        SetSingleton(sequence);
    }
}