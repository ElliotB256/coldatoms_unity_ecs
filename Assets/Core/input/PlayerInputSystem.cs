using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System;

/// <summary>
/// A system that monitors the player key input
/// </summary>
public class PlayerInputSystem : JobComponentSystem
{
    /// <summary>
    /// This runs on main thread and can get keyboard input state.
    /// </summary>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        PlayerInputs actualinput = new PlayerInputs();
        actualinput.VerticalAxis = Input.GetAxis("Vertical");

        return Entities.ForEach((ref PlayerInputs input) => input = actualinput).Schedule(inputDeps);
    }
}

/// <summary>
/// Arrow key inputs
/// </summary>
[Serializable]
public struct PlayerInputs : IComponentData
{
    public float VerticalAxis;
}
