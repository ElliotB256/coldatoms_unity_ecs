using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[AlwaysUpdateSystem]
public class ResetSystem : SystemBase
{
    EntityQuery AtomQuery;
    private bool _lastInputState = false;

    protected override void OnCreate()
    {
        AtomQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {
                    ComponentType.ReadOnly<Atom>(),
                }
        });
    }

    protected override void OnUpdate()
    {
        bool resetButton = Input.GetButtonDown("Reset");
        if (resetButton && !_lastInputState)
        {
            EntityManager.DestroyEntity(AtomQuery);
            Entities.ForEach((ref AtomCloud cloud) => cloud.ShouldSpawn = true).Schedule();
        }
        _lastInputState = resetButton;
    }
}