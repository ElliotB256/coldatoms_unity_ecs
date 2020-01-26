using System;
using Unity.Entities;

namespace Calculation
{
    public struct Group : IComponentData {
        public eGroup Mask;

        public const int NUMBER_OF_GROUPS = 8;
    }

    [Flags]
    public enum eGroup : byte
    {
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,
        D = 1 << 3,
        E = 1 << 4,
        F = 1 << 5,
        G = 1 << 6,
        H = 1 << 7,
        All = byte.MaxValue,
        None = 0
    }
}
