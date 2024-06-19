using EcsExtension;
using TowerDefense.Components;
using TowerDefense.Components.Projectile;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace TowerDefense.Systems.Projectile
{
    public partial struct ProjectileAttackSystem : ISystem
    {
        private EntityQuery _projectileQuery;

        public void OnCreate(ref SystemState state)
        {
            _projectileQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadWrite<ProjectileComponent>(),
                    ComponentType.ReadWrite<LocalTransform>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadOnly<TargetEntity>(), 
                }
            });
        }

        public void OnUpdate(ref SystemState state)
        {
            JobHandle dependency = state.Dependency;

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            dependency = new ProjectileMoveToTargetJob()
            {
                ecbWriter = ecb.AsParallelWriter(),
                localTransformLookup = state.GetComponentLookup<LocalTransform>(),
                damageBufferLookup = state.GetBufferLookup<DamageBufferElement>(),
                localToWorldLookup = state.GetComponentLookup<LocalToWorld>(),
                dt = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(_projectileQuery, dependency);
                
            dependency.Complete();
            ecb.Playback(state.EntityManager);
                
            ecb.Dispose();    
        }
        
        public partial struct ProjectileMoveToTargetJob : IJobEntity
        {
            [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> localTransformLookup;
            [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;
            [NativeDisableParallelForRestriction] public BufferLookup<DamageBufferElement> damageBufferLookup;

            public EntityCommandBuffer.ParallelWriter ecbWriter;
            public float dt;
            
            public void Execute(
                Entity entity,
                in ProjectileComponent projectileComponent,
                in LocalToWorld localToWorld,
                in TargetEntity targetEntity,
                [ChunkIndexInQuery] int chunkIndex
                )
            {
                if (!localTransformLookup.HasComponent(targetEntity.value))
                {
                    //ecbWriter.Instantiate(chunkIndex, projectileComponent.explodeParticlePrefab);
                    ecbWriter.DestroyEntity(chunkIndex, entity);
                    return;
                }
                
                if(!localTransformLookup.HasComponent(entity)) return;
                
                float3 worldPos = localToWorld.Position;
                float3 targetWorldPos = localToWorldLookup.GetRefRO(targetEntity.value).ValueRO.Position;

                float projectileTravelDistanceSq = math.pow(projectileComponent.speed * dt, 2);
                float distanceLeftSq = math.distancesq(worldPos, targetWorldPos);

                if (distanceLeftSq.HasReachedDestination(projectileTravelDistanceSq, 0.1f*0.1f))
                {
                    //ecbWriter.Instantiate(chunkIndex, projectileComponent.explodeParticlePrefab);
                    ecbWriter.DestroyEntity(chunkIndex, entity);

                    if (damageBufferLookup.HasBuffer(targetEntity.value))
                    {
                        ecbWriter.AppendToBuffer(chunkIndex, targetEntity.value, new DamageBufferElement()
                        {
                            value = projectileComponent.damage
                        });
                    }
                }
                else
                {
                    ref var localTransform = ref localTransformLookup.GetRefRW(entity).ValueRW;
                    float3 direction = math.normalizesafe(targetWorldPos - worldPos);
                    localTransform.Position += direction * projectileComponent.speed * dt;
                }
                
            }
        }
    }
}