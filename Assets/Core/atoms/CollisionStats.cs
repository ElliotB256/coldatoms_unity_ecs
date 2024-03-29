﻿using System;
using Unity.Entities;

[Serializable]
public struct CollisionStats : IComponentData
{
    /// <summary>
    /// Time since last collision, in seconds
    /// </summary>
    public float TimeSinceLastCollision;

    /// <summary>
    /// Distance since last collision, in seconds
    /// </summary>
    public float DistanceSinceLastCollision;

    /// <summary>
    /// Bool true is collision this frame
    /// </summary>
    public bool CollidedThisFrame;
}
