using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct WallCollisions : IComponentData
{
    public float Impulse;
        // Multiple wall collisions in sinlge frame possible?
    public int WallIndex;
}
