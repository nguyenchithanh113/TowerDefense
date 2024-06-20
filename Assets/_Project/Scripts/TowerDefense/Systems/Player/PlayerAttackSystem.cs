using TowerDefense.Components;
using TowerDefense.Components.Gun;
using TowerDefense.Components.Player;
using TowerDefense.Components.Projectile;
using TowerDefense.Components.Turret;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace TowerDefense.Systems.Player
{
    public partial struct PlayerAttackSystem : ISystem
    {
        private EntityQuery _playerShootQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerSettingComponent>();
            
            _playerShootQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<PlayerTag>(),
                    ComponentType.ReadOnly<LocalTransform>(),
                    ComponentType.ReadWrite<Shoot>()
                }
            });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            PlayerSettingComponent playerSettingComponent = SystemAPI.GetSingleton<PlayerSettingComponent>();

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            state.Dependency = new PlayerShootJob()
            {
                ecb = ecb,
                projectilePrefab = playerSettingComponent.projectilePrefab,
                dt = SystemAPI.Time.DeltaTime
            }.Schedule(_playerShootQuery,state.Dependency);
            
            state.Dependency.Complete();
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
        
        [BurstCompile]
        public partial struct PlayerShootJob : IJobEntity
        {
            public EntityCommandBuffer ecb;

            public Entity projectilePrefab;
            public float dt;

            public void Execute(
                ref Shoot shoot,
                in LocalTransform localTransform)
            {
                if (shoot.cooldownTimer <= 0)
                {
                    Entity projectileEntity = ecb.Instantiate(projectilePrefab);

                    LocalTransform projectileLocalTransform = new LocalTransform()
                    {
                        Position = localTransform.Position,
                        Rotation = localTransform.Rotation,
                        Scale = 1f
                    };
                    
                    ecb.AddComponent(projectileEntity, projectileLocalTransform);

                    shoot.cooldownTimer = shoot.cooldown;
                }
                else
                {
                    shoot.cooldownTimer -= dt;
                }
            }
        }
    }
}