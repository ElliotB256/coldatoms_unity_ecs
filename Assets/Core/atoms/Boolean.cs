using System;

/// <summary>
/// Blittable boolean type. see https://playerone-studio.com/blittable-bool-unity-ecs
/// </summary>
[Serializable]
public struct Boolean
{
    public byte boolValue;

    public Boolean(bool value)
    {
        boolValue = (byte)(value ? 1 : 0);
    }

    public static implicit operator Boolean(bool value)
    {
        return new Boolean(value);
    }

    public bool ToBool()
    {
        if (boolValue == 1)
            return true;

        return false;
    }
}