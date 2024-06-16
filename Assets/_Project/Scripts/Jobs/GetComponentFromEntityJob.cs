using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    public struct GetComponentFromEntityForLoopJob<T> : IJobFor where T : unmanaged, IComponentData
    {
        [NativeDisableParallelForRestriction] public NativeArray<T> componentDataArr;
        [NativeDisableParallelForRestriction] public NativeList<Entity> entityList;
        [NativeDisableParallelForRestriction] public ComponentLookup<T> componentLookup;

        public void Execute(int index)
        {
            var entity = entityList[index];

            componentDataArr[index] = componentLookup.GetRefRO(entity).ValueRO;
        }
    }
    
}