using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;


/// <summary>
/// A system that monitors the player key input
/// </summary>
public class PlayerInputSystem : JobComponentSystem
{

    struct PlayerInputJob : IJobForEach<PlayerInputs>
    {
        public Boolean upbutton;
        public Boolean downbutton;

        public void Execute(ref PlayerInputs data)
        {
            data.UpButton = upbutton;
            data.DownButton = downbutton;
        }
    }

    /// <summary>
    /// This runs on main thread and can get keyboard input state.
    /// </summary>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new PlayerInputJob
        {
            upbutton = new Boolean(Input.GetKeyDown(KeyCode.UpArrow)),
            downbutton = new Boolean(Input.GetKeyDown(KeyCode.DownArrow))
        };
        return job.Schedule(this, inputDeps);
    }

}
