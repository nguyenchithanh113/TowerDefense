using EcsExtension;
using Jobs;
using TowerDefense.Components.Enemy;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace TowerDefense.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(BeginSimulationEntityCommandBufferSystem))]
    public partial struct InputSystem : ISystem
    {
        public bool IsMouseDown;
        public Vector2 MousePosition;

        public void OnCreate(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {
            IsMouseDown = InputSystemCapture.IsMouseDown;
            MousePosition = InputSystemCapture.MousePosition;

            if (IsMouseDown)
            {
                EntityQuery entityQuery = state.GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());

                var enemyTagArrRW =
                    new NativeArray<EnemyTag>(entityQuery.CalculateEntityCount(), Allocator.TempJob);
                var entityList = entityQuery.ToEntityListAsync(Allocator.TempJob, out var jobHandle);
                state.Dependency = jobHandle;

                state.Dependency = new GetComponentFromEntityForLoopJob<EnemyTag>()
                {
                    componentDataArr = enemyTagArrRW,
                    entityList = entityList,
                    componentLookup = state.GetComponentLookup<EnemyTag>()
                }.ScheduleParallel(enemyTagArrRW.Length, 64, state.Dependency);

                state.Dependency.Complete();

                for (int i = 0; i < enemyTagArrRW.Length; i++)
                {
                    Debug.Log(enemyTagArrRW[i].index);
                }

                enemyTagArrRW.Dispose();
                entityList.Dispose();
            }
        }
    }
}