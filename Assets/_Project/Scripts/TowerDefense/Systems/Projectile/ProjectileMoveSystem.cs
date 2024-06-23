using TowerDefense.Components;
using TowerDefense.Components.Projectile;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;

namespace TowerDefense.Systems.Projectile
{
    public partial struct ProjectileMoveSystem : ISystem
    {
        private EntityQuery _projectileQuery;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            
            _projectileQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadWrite<LocalTransform>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadWrite<ProjectileComponent>(),
                }
            });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            JobHandle dependency = state.Dependency;

            PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            dependency = new ProjectileMoveJob()
            {
                collisionWorld = physicsWorld.CollisionWorld,
                damageBufferLookup = state.GetBufferLookup<DamageBufferElement>(),
                dt = SystemAPI.Time.DeltaTime,
                ecbWriter = ecb.AsParallelWriter(),
            }.ScheduleParallel(_projectileQuery, dependency);
            
            dependency.Complete();
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        
        public void OnDestroy(ref SystemState state)
        {

        }
    }
    
    [BurstCompile]
    public partial struct ProjectileMoveJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecbWriter;
        [ReadOnly] public CollisionWorld collisionWorld;

        [NativeDisableParallelForRestriction] public BufferLookup<DamageBufferElement> damageBufferLookup;

        public float dt;
        
        public void Execute(
            Entity entity,
            ref LocalTransform localTransform,
            in LocalToWorld localToWorld, 
            ref ProjectileComponent projectileComponent,
            [ChunkIndexInQuery] int chunkIndex)
        {
            
            localTransform.Position += localTransform.Forward() * projectileComponent.speed * dt;

            RaycastInput raycastInput = new RaycastInput()
            {
                Start = projectileComponent.lastPosition,
                End = localTransform.Position,
                Filter = CollisionFilter.Default
            };
            if (collisionWorld.CastRay(raycastInput, out RaycastHit hit))
            {
                if (damageBufferLookup.HasBuffer(hit.Entity))
                {
                    ecbWriter.AppendToBuffer(chunkIndex, hit.Entity, new DamageBufferElement()
                    {
                        value = projectileComponent.damage
                    });
                }
                ecbWriter.DestroyEntity(chunkIndex,entity);
            }
            else
            {
                projectileComponent.lastPosition = localTransform.Position;
            }
        }
    }
}