using EcsExtension;
using TowerDefense.Components;
using TowerDefense.Components.Enemy;
using TowerDefense.Components.Gun;
using TowerDefense.Components.Tower;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace TowerDefense.Systems.Tower
{
    public partial struct TowerShootSystem : ISystem, ISystemStartStop
    {
        private EntityQuery _towerQuery;
        private EntityQuery _towerWithGunQuery;

        public void OnCreate(ref SystemState state)
        {
            _towerQuery = state.GetEntityQuery(new[]
            {
                ComponentType.ReadOnly<TowerTag>(),
            });

            _towerWithGunQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<TowerTag>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadWrite<GunBufferElement>()
                }
            });

            state.RequireForUpdate<GameSettingComponent>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var dependency = state.Dependency;

            GameSettingComponent gameSettingComponent = SystemAPI.GetSingleton<GameSettingComponent>();

            var addGunBufferEcb = new EntityCommandBuffer(Allocator.TempJob);

            dependency = new AddGunBufferForTowerIfNotExistJob()
            {
                gunBuffer = state.GetBufferLookup<GunBufferElement>(),
                parallelWriter = addGunBufferEcb.AsParallelWriter(),
                gunPrefab = gameSettingComponent.normalGunPrefab,
            }.ScheduleParallel(_towerQuery, dependency);

            dependency.Complete();

            addGunBufferEcb.Playback(state.EntityManager);

            addGunBufferEcb.Dispose();
            state.Dependency = dependency;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            JobHandle dependency = state.Dependency;
            PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            dependency = new CheckTargetValidityForTowerGunsJob()
            {
                ecbWriter = ecb.AsParallelWriter(),
                localTransformLookup = state.GetComponentLookup<LocalTransform>(),
                targetEntityLookup = state.GetComponentLookup<TargetEntity>(),
                localToWorldLookup = state.GetComponentLookup<LocalToWorld>()
            }.ScheduleParallel(_towerWithGunQuery, dependency);
            dependency.Complete();
            
            dependency = new FindTargetForTowerGunsJob()
            {
                ecbWriter = ecb.AsParallelWriter(),
                targetEntityLookup = state.GetComponentLookup<TargetEntity>(),
                physicsWorld = physicsWorld.PhysicsWorld,
                enemyTagLookup = state.GetComponentLookup<EnemyTag>(),
            }.ScheduleParallel(_towerWithGunQuery, dependency);
            dependency.Complete();
            
            dependency = new AttackTargetForTowerGunsJob()
            {
                dt = SystemAPI.Time.DeltaTime,
                ecbWrite = ecb.AsParallelWriter(),
                targetEntityLookup = state.GetComponentLookup<TargetEntity>(),
                //damageBufferLookup = state.GetBufferLookup<DamageBufferElement>(),
                gunComponentLookup = state.GetComponentLookup<GunComponent>(),
            }.ScheduleParallel(_towerWithGunQuery, dependency);
            dependency.Complete();
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            state.Dependency = dependency;
        }

        public void OnStopRunning(ref SystemState state)
        {
        }
    }

    [BurstCompile]
    public partial struct AddGunBufferForTowerIfNotExistJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public BufferLookup<GunBufferElement> gunBuffer;

        public EntityCommandBuffer.ParallelWriter parallelWriter;
        public Entity gunPrefab;

        public void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex)
        {
            if (!gunBuffer.TryGetBuffer(entity, out DynamicBuffer<GunBufferElement> buffer))
            {
                parallelWriter.AddBuffer<GunBufferElement>(chunkIndex, entity);

                Entity gun = parallelWriter.Instantiate(chunkIndex, gunPrefab);
                parallelWriter.AddComponent(chunkIndex, gun, new Parent()
                {
                    Value = entity
                });
                parallelWriter.AddComponent(chunkIndex, gun, LocalTransform.FromPosition(0f, 0f, 0f));

                GunBufferElement gunBufferElement = new GunBufferElement()
                {
                    entity = gun
                };

                parallelWriter.AppendToBuffer<GunBufferElement>(chunkIndex, entity, gunBufferElement);
            }
        }
    }

    [BurstCompile]
    public partial struct CheckTargetValidityForTowerGunsJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalTransform> localTransformLookup;
        [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;
        [ReadOnly] public ComponentLookup<TargetEntity> targetEntityLookup;
        public EntityCommandBuffer.ParallelWriter ecbWriter;

        public void Execute(in LocalToWorld localToWorld,
            ref DynamicBuffer<GunBufferElement> gunBuffer, [ChunkIndexInQuery] int chunkIndex)
        {
            for (int i = 0; i < gunBuffer.Length; i++)
            {
                Entity gunEntity = gunBuffer[i].entity;
                if (targetEntityLookup.HasComponent(gunEntity))
                {
                    TargetEntity targetEntity = targetEntityLookup.GetRefRO(gunEntity).ValueRO;
                    bool isTargetValid = true;
                        
                    if (!JobsHelper.IsEntityWithTransformExist(localTransformLookup, targetEntity.value))
                    {
                        isTargetValid = false;
                    }
                    else
                    {
                        LocalToWorld targetLtw = localToWorldLookup.GetRefRO(targetEntity.value).ValueRO;
                        float distanceToTargetSq = math.distancesq(targetLtw.Position, localToWorld.Position);

                        if (distanceToTargetSq > 5.8f * 5.8f)
                        {
                            isTargetValid = false;
                        }
                    }

                    if (!isTargetValid)
                    {
                        ecbWriter.RemoveComponent<TargetEntity>(chunkIndex, gunEntity);
                            //Debug.Log("Remove target");
                    }
                }
            }
        }
    }

    [BurstCompile]
    public partial struct FindTargetForTowerGunsJob : IJobEntity
    {
        [ReadOnly] public PhysicsWorld physicsWorld;
        [ReadOnly] public ComponentLookup<EnemyTag> enemyTagLookup;
        [ReadOnly] public ComponentLookup<TargetEntity> targetEntityLookup;

        public EntityCommandBuffer.ParallelWriter ecbWriter;

        public void Execute(in LocalToWorld localToWorld, in DynamicBuffer<GunBufferElement> gunBuffer,
            [ChunkIndexInQuery] int chunkIndex)
        {
            for (int gunIndex = 0; gunIndex < gunBuffer.Length; gunIndex++)
            {
                Entity gunEntity = gunBuffer[gunIndex].entity;

                if (!targetEntityLookup.HasComponent(gunEntity))
                {
                    NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.TempJob);
                    
                    physicsWorld.OverlapSphere(localToWorld.Position, 5, ref hits, CollisionFilter.Default);

                    float minDis = float.MaxValue;
                    Entity closestEntity = Entity.Null;

                    for (int i = 0; i < hits.Length; i++)
                    {
                        DistanceHit hit = hits[i];
                        Entity hitEntity = hit.Entity;
                        float dis = hit.Distance;

                        if (enemyTagLookup.HasComponent(hitEntity) && dis < minDis)
                        {
                            minDis = dis;
                            closestEntity = hitEntity;
                        }
                    }

                    if (closestEntity != Entity.Null)
                    {
                        TargetEntity targetEntity = new TargetEntity()
                        {
                            value = closestEntity
                        };

                        ecbWriter.AddComponent(chunkIndex, gunEntity, targetEntity);
                    }

                    hits.Dispose();
                }
            }
        }
    }
    
    [BurstCompile]
    public partial struct AttackTargetForTowerGunsJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<TargetEntity> targetEntityLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<GunComponent> gunComponentLookup;

        public EntityCommandBuffer.ParallelWriter ecbWrite;

        public float dt;
        public void Execute(in DynamicBuffer<GunBufferElement> gunBuffer,
            [ChunkIndexInQuery] int chunkIndex)
        {
            for (int i = 0; i < gunBuffer.Length; i++)
            {
                Entity gunEntity = gunBuffer[i].entity;
                ref GunComponent gunComponentRW = ref gunComponentLookup.GetRefRW(gunEntity).ValueRW;

                if (gunComponentRW.cooldownTimer <= 0)
                {
                    if (targetEntityLookup.TryGetComponent(gunEntity, out TargetEntity targetEntity))
                    {
                        Entity projectilePrefab = ecbWrite.Instantiate(chunkIndex, gunComponentRW.projectilePrefab);
                        ecbWrite.AddComponent(chunkIndex, projectilePrefab, new TargetEntity()
                        {
                            value = targetEntity.value
                        });
                        ecbWrite.SetComponent(chunkIndex, projectilePrefab, LocalTransform.FromPosition(0f,0.5f,0f));
                        
                        gunComponentRW.cooldownTimer = gunComponentRW.cooldown;
                    }
                }

                gunComponentRW.cooldownTimer -= dt;
            }
        }
    }
}