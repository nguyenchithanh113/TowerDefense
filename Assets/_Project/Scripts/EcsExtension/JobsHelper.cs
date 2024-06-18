using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

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

        public static bool IsEntityWithTransformExist(ComponentLookup<LocalTransform> componentLookup, Entity entity)
        {
            return componentLookup.HasComponent(entity);
        }
        // public static bool IsEntityWithTransformExist(ComponentLookup<LocalToWorld> componentLookup, Entity entity)
        // {
        //     return componentLookup.HasComponent(entity);
        // }
    }
}