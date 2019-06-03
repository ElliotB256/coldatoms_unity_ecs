﻿using System;
using Unity.Entities;

/// <summary>
/// Arrow key inputs
/// </summary>
[Serializable]
public struct PlayerInputs : IComponentData
{
    public Boolean UpButton;
    public Boolean DownButton;
}
