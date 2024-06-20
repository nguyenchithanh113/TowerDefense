using TowerDefense.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace TowerDefense.Systems
{
    public partial struct HealthDamageSystem : ISystem
    {
        private EntityQuery _healthDamageQuery;
        
        public void OnCreate(ref SystemState state)
        {
            _healthDamageQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadWrite<HealthComponent>(),
                    ComponentType.ReadWrite<DamageBufferElement>(),
                }
            });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            JobHandle dependency = state.Dependency;

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            dependency = new HealthDamageCalculationJob()
            {
            }.ScheduleParallel(_healthDamageQuery, dependency);

            dependency = new HealthCheckJob()
            {
                ecbWriter = ecb.AsParallelWriter()
            }.ScheduleParallel(_healthDamageQuery, dependency);
            
            dependency.Complete();
            ecb.Playback(state.EntityManager);
            
            ecb.Dispose();

            state.Dependency = dependency;
        }
    }

    [BurstCompile]
    public partial struct HealthDamageCalculationJob : IJobEntity
    {
        public void Execute(ref HealthComponent healthComponent,
            ref DynamicBuffer<DamageBufferElement> damageBuffer)
        {
            for (int i = 0; i < damageBuffer.Length; i++)
            {
                healthComponent.value -= damageBuffer[i].value;
            }

            damageBuffer.Clear();
        }
    }

    [BurstCompile]
    public partial struct HealthCheckJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecbWriter;

        public void Execute(Entity entity, in HealthComponent healthComponent, [ChunkIndexInQuery] int chunkIndex)
        {
            if (healthComponent.value <= 0)
            {
                ecbWriter.DestroyEntity(chunkIndex, entity);
            }
        }
    }
}