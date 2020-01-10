using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECSUtil
{
    /// <summary>
    /// Builds a NativeArray of the specified component type.
    /// </summary>
    public struct GetNativeArrayJob<T> : IJobForEachWithEntity<T> where T : struct, IComponentData
    {
        public NativeArray<T> Array;

        public void Execute(Entity entity, int index, ref T c0)
        {
            Array[index] = c0;
        }
    }
}
