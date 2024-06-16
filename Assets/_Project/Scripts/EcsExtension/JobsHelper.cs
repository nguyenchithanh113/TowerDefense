using Unity.Collections;
using Unity.Entities;

namespace EcsExtension
{
    public static class JobsHelper
    {
        public static NativeArray<T> GetNativeArrayHolderFromChunks<T>(NativeArray<ArchetypeChunk> chunks, Allocator allocator) where T : unmanaged, IComponentData
        {
            int count = 0;

            for (int i = 0; i < chunks.Length; i++)
            {
                count += chunks[i].Count;
            }

            return new NativeArray<T>(count, allocator);
        }
    }
}