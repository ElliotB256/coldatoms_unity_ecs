using Integration;
using Unity.Entities;
using UnityEngine;

public class PauseTimeSystem : SystemBase
{
    private bool _isPaused = false;
    private bool _lastInputState = false;
    FixedUpdateGroup _SystemGroup = null;

    protected override void OnCreate()
    {
        _SystemGroup = World.GetOrCreateSystem<FixedUpdateGroup>();
    }

    protected override void OnUpdate()
    {
        //toggle pause state if pause button is pressed.
        bool pauseButton = Input.GetButton("Pause");
        if (pauseButton && !_lastInputState)
            _isPaused = !_isPaused;
        _lastInputState = pauseButton;

        _SystemGroup.IsPaused = _isPaused;
    }
}