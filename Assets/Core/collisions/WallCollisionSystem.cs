using Integration;
using System;
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

    protected override void OnCreateManager()
    {
        WallQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                    ComponentType.ReadOnly<InfinitePlane>(),
                }
        });
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        float DeltaTime = FixedUpdateGroup.FIXED_TIME_DELTA;
        NativeArray<InfinitePlane> Walls = WallQuery.ToComponentDataArray<InfinitePlane>(Allocator.TempJob);
        return new UpdatePositionWithWallJob
        {
            dT = DeltaTime,
            Walls = Walls
        }.Schedule(this, inputDependencies);
    }

    [BurstCompile]
    [RequireComponentTag(typeof(Atom))]
    struct UpdatePositionWithWallJob : IJobForEachWithEntity<Translation, Velocity, Mass, PrevForce>
    {
        public float dT;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<InfinitePlane> Walls;

        public void Execute(
            Entity entity,
            int index,
            ref Translation translation,
            ref Velocity velocity,
            [ReadOnly] ref Mass mass,
            [ReadOnly] ref PrevForce force)
        {
            // Normalise delta
            // Test for collision against all walls
            // Take wall that is nearest, collide off it
            // Repeat until no collisions.
            var delta = velocity.Value * dT + 0.5f * (force.Value / mass.Value) * dT * dT;
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

                    //move atom away from wall
                    translation.Value += 1.0e-4f * normal * math.dot(direction, normal);
                }
                else
                {
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
}

/// <summary>
/// Represents a triangular segment of wall that atoms can deflect off.
/// </summary>
[Serializable]
public struct InfinitePlane : IComponentData
{
    public float3 V1;
    public float3 Normal;
}
