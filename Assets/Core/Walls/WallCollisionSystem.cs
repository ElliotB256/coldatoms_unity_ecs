using Integration;
using System;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// Update position according to velocity verlet.
/// </summary>
[AlwaysUpdateSystem]
[UpdateBefore(typeof(ForceCalculationSystems))]
[UpdateInGroup(typeof(FixedUpdateGroup))]
public class UpdatePositionWithWallSystem : JobComponentSystem
{
    EntityQuery WallQuery;
    EntityQuery AtomQuery;
        // Don't actually know what this does but it seems to fix the issue
    [NativeDisableParallelForRestriction] NativeArray<bool> Collided;


    protected override void OnCreate()
    {
        WallQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                    ComponentType.ReadOnly<InfinitePlane>(),
                    ComponentType.ReadOnly<WIndex>(),
                    ComponentType.ReadOnly<WZ1Index>(),
                    ComponentType.ReadOnly<WZ2Index>()
                }
        });

        AtomQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]  {
                typeof(CollisionStats),
                ComponentType.ReadOnly<Atom>()
            }
        });
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
            // Form an array for Collided particles
        int atomNumber = AtomQuery.CalculateEntityCount();
        Collided = new NativeArray<bool>(atomNumber, Allocator.TempJob, NativeArrayOptions.ClearMemory);

        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        NativeArray<InfinitePlane> Walls = WallQuery.ToComponentDataArray<InfinitePlane>(Allocator.TempJob);
        NativeArray<WIndex> WallIndices = WallQuery.ToComponentDataArray<WIndex>(Allocator.TempJob);
        NativeArray<WZ1Index> WallZ1Indices = WallQuery.ToComponentDataArray<WZ1Index>(Allocator.TempJob);
        NativeArray<WZ2Index> WallZ2Indices = WallQuery.ToComponentDataArray<WZ2Index>(Allocator.TempJob);

        var firstJob = new UpdatePositionWithWallJob
        {
            dT = DeltaTime,
            Walls = Walls,
            WallIndices = WallIndices,
            WallZ1Indices = WallZ1Indices,
            WallZ2Indices = WallZ2Indices,
            Collided = Collided
        }.Schedule(this, inputDependencies);

        var secondJob =  new UpdateCollisionStatsJob
        {
            Collided = Collided
        }.Schedule(AtomQuery, firstJob);

        Collided.Dispose(secondJob);

        return secondJob;
    }

    [BurstCompile]
    [RequireComponentTag(typeof(Atom))]
    struct UpdatePositionWithWallJob : IJobForEachWithEntity<Translation, Velocity, WallCollisions, Mass, PrevForce, Zone>
    {
        public float dT;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<InfinitePlane> Walls;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<WIndex> WallIndices;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<WZ1Index> WallZ1Indices;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<WZ2Index> WallZ2Indices;
        public NativeArray<bool> Collided;

        public void Execute(
            Entity entity,
            int index,
            ref Translation translation,
            ref Velocity velocity,
            ref WallCollisions wallCollisions,
            [ReadOnly] ref Mass mass,
            [ReadOnly] ref PrevForce force,
            [ReadOnly] ref Zone zone)
        {
                // dx = v*dt + 0.5 * a * dt^2

            var delta = velocity.Value * dT + 0.5f * (force.Value / mass.Value) * dT * dT;

            if (Walls.Length == 0)
            {
                translation.Value = translation.Value + delta;
                return;

            }

            // Normalise delta
            // Test for collision against all walls
            // Take wall that is nearest, collide off it
            // Repeat until no collisions.
            
            var remaining = math.length(delta);
            var direction = delta / remaining;

            while (remaining > 0f)
            {
                // Find nearest wall
                var distance = float.PositiveInfinity;
                var wallIndex = 0;
                for (int i = 0; i < Walls.Length; i++)
                {
                    var newDistance = GetDistance(ref translation, ref direction, Walls[i]);
                    if (newDistance < distance && newDistance > 0f)
                    {
                        distance = newDistance;
                        wallIndex = i;
                    }
                }

                if (distance <= remaining)
                {
                    // Collision occurs - move atom to the wall, change direction.
                    translation.Value += distance * direction;
                    remaining = remaining - distance;
                    
                    var normal = Walls[wallIndex].Normal;
                    direction = math.normalize(direction - 2 * math.dot(direction, normal) * normal);

                        // Pressure on each zone of the wall
                    if (zone.Value == 0) 
                    {
                        wallCollisions.WallIndex = WallIndices[wallIndex].Value;
                    }
                    else if (zone.Value == 1)
                    {
                        wallCollisions.WallIndex = WallZ1Indices[wallIndex].Value;
                    }
                    else if (zone.Value == 2)
                    {
                        wallCollisions.WallIndex = WallZ2Indices[wallIndex].Value;
                    }

                    wallCollisions.Impulse = 2*Mathf.Abs(mass.Value*math.dot(velocity.Value, normal));

                        // Set the particle to have collided this frame
                    Collided[index] = true;

                    // instead of moving the atom away from the wall, just check that it is moving towards the wall for a collision to occur 

                    //move atom away from wall
                        // I think this is causing some leaks, very low number so may not be worth fixing 
                    translation.Value += 1.0e-4f * normal * math.dot(direction, normal);
                }
                else
                {
                        // Normal motion free from collision
                    translation.Value = translation.Value + direction * remaining;
                    remaining = 0f;
                }
            }

            velocity.Value = math.length(velocity.Value) * direction;
        }

        /// <summary>
        /// Calculates the distance until intersection
        /// </summary>
        /// <returns>Distance between point 'translation' and wall surface, travelling along direction.</returns>
        float GetDistance(ref Translation translation, ref float3 direction, InfinitePlane wall)
        {
            float dot1 = math.dot(direction, wall.Normal);
            float dot2 = math.dot(wall.V1 - translation.Value, wall.Normal);
            if (dot1 == 0) return float.PositiveInfinity;
            float distance = dot2 / dot1;
            return distance;
        }
    }

        // Job to update the collision statistics 
    [BurstCompile]
    struct UpdateCollisionStatsJob : IJobForEachWithEntity<CollisionStats>
    {
        public NativeArray<bool> Collided;

        public void Execute(Entity entity, int index, ref CollisionStats stats)
        {
            if (Collided[index])
                // stats.TimeSinceLastCollision = 0f;
                stats.CollidedThisFrame = true;
        }
    }

}

