using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System;

/// <summary>
/// A system that monitors the player key input
/// </summary>
public class PlayerInputSystem : SystemBase
{
    /// <summary>
    /// This runs on main thread and can get keyboard input state.
    /// </summary>
    protected override void OnUpdate()
    {
        PlayerInputs actualinput = new PlayerInputs();
        actualinput.VerticalAxis = Input.GetAxis("Vertical");

        Entities.ForEach((ref PlayerInputs input) => input = actualinput).Schedule();
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
