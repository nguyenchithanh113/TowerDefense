using TowerDefense.Components;
using TowerDefense.Components.Gun;
using TowerDefense.Components.Tower;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;

namespace TowerDefense.Systems.Tower
{
    public partial struct TowerShootSystem : ISystem, ISystemStartStop
    {
        private EntityQuery _towerQuery;
        
        public void OnCreate(ref SystemState state)
        {
            _towerQuery = state.GetEntityQuery(new[]
            {
                ComponentType.ReadOnly<TowerTag>(),
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

        public void OnUpdate(ref SystemState state)
        {
            PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorld.CollisionWorld;
            
            collisionWorld.OverlapSphere()
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
                parallelWriter.AddComponent(chunkIndex, gun, LocalTransform.FromPosition(0f,0f,0f));
                
                GunBufferElement gunBufferElement = new GunBufferElement()
                {
                    entity = gun
                };
                
                parallelWriter.AppendToBuffer<GunBufferElement>(chunkIndex, entity, gunBufferElement);
            }
        }
    }
}