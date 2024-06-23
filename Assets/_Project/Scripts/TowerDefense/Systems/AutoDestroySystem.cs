using TowerDefense.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace TowerDefense.Systems
{
    public partial struct AutoDestroySystem : ISystem
    {
        private EntityQuery _autoDestroyQuery;
        
        public void OnCreate(ref SystemState state)
        {
            _autoDestroyQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadWrite<AutoDestroyComponent>(),
                }
            });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            JobHandle dependency = state.Dependency;

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            dependency = new AutoDestroyJob()
            {
                ecbWrite = ecb.AsParallelWriter(),
                dt = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(_autoDestroyQuery, dependency);
            
            dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }


        public void OnDestroy(ref SystemState state)
        {
            //LinkedEntityGroup
        }
    }
    
    [BurstCompile]
    public partial struct AutoDestroyJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecbWrite;
        public float dt;
        
        public void Execute(
            Entity entity,
            ref AutoDestroyComponent autoDestroyComponent,
            [ChunkIndexInQuery] int chunkIndex)
        {
            if (autoDestroyComponent.timer <= 0)
            {
                ecbWrite.DestroyEntity(chunkIndex,entity);
            }
            else
            {
                autoDestroyComponent.timer -= dt;
            }
        }
    }
}