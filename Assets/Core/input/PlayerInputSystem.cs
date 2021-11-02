using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using System;
using Unity.Mathematics;

/// <summary>
/// A system that monitors the player key input
/// </summary>
public class PlayerInputSystem : SystemBase
{
    public const float DOUBLE_CLICK_DELAY = 0.2f;
    /// <summary>
    /// This runs on main thread and can get keyboard input state.
    /// </summary>
    protected override void OnUpdate()
    {
        var vertical = Input.GetAxis("Vertical");
        var clicked = Input.GetMouseButtonDown(0);
        var dT = Time.DeltaTime;
        var mousePos = Input.mousePosition;
        var buttonDown = Input.GetMouseButton(0);
        var sequence = GetSingleton<Sequence>();

        Entities.ForEach((ref PlayerInputs input) =>
        {
            input.VerticalAxis = vertical;
            input.doubleClicked = false;
            input.clicked = clicked;
            
            if (clicked && input.lastClickTime < DOUBLE_CLICK_DELAY)
                input.doubleClicked = true;

            input.lastClickTime += dT;

            if (clicked)
            {
                input.lastClickTime = 0f;
                input.clickPosition = mousePos;
                if (sequence.Stage == SequenceStage.WaitAfterReadout)
                    input.advance = true;
            }

            input.buttonDown = buttonDown;
        }).Schedule();
    }
}

[Serializable]
public struct PlayerInputs : IComponentData
{
    public float VerticalAxis;
    public float lastClickTime;
    public bool doubleClicked;
    public bool clicked;
    public float3 clickPosition;
    public bool buttonDown;
    public bool advance;
}
