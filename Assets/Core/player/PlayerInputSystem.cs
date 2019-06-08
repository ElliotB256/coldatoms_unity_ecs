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
        public PlayerInputs input;

        public void Execute(ref PlayerInputs data)
        {
            data = input;
        }
    }

    /// <summary>
    /// This runs on main thread and can get keyboard input state.
    /// </summary>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        PlayerInputs input = new PlayerInputs();
        input.VerticalAxis = Input.GetAxis("Vertical");
        var job = new PlayerInputJob { input = input };
        return job.Schedule(this, inputDeps);
    }
}
