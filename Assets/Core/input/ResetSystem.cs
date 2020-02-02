using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[AlwaysUpdateSystem]
public class ResetSystem : JobComponentSystem
{
    EntityQuery AtomQuery;
    private bool _lastInputState = false;

    protected override void OnCreateManager()
    {
        AtomQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                    ComponentType.ReadOnly<Atom>(),
                }
        });
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        bool resetButton = Input.GetButton("Reset");
        if (resetButton && !_lastInputState)
        {
            EntityManager.DestroyEntity(AtomQuery);
            return Entities.ForEach((ref AtomCloud cloud) => cloud.ShouldSpawn = true).Schedule(inputDependencies);
        }
        _lastInputState = resetButton;
        return inputDependencies;
    }
}