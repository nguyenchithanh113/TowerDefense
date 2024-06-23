using ProjectDawn.Mathematics;
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
                    float3 forward = localTransform.Forward();

                    NativeArray<float2> directionArray = new NativeArray<float2>(9, Allocator.Temp);


                    int count = 0;
                    for (int i = -40; i <= 40; i += 10)
                    {
                        directionArray[count] = math2.rotate(new float2(forward.x, forward.z), math.TORADIANS * i);
                        count++;
                    }

                    for (int i = 0; i < 9; i++)
                    {
                        float3 direction = new float3(directionArray[i].x, 0, directionArray[i].y);
                        quaternion rot = quaternion.LookRotation(direction, math.up());
                        
                        Entity projectileEntity = ecb.Instantiate(projectilePrefab);

                        LocalTransform projectileLocalTransform = new LocalTransform()
                        {
                            Position = new float3(localTransform.Position.x, 0.5f, localTransform.Position.z),
                            Rotation = rot,
                            Scale = 1f
                        };
                    
                        ecb.AddComponent(projectileEntity, projectileLocalTransform);
                        ecb.AddComponent(projectileEntity, new AutoDestroyComponent()
                        {
                            timer = 10,
                        });
                    }

                    shoot.cooldownTimer = shoot.cooldown;

                    directionArray.Dispose();
                }

                shoot.cooldownTimer -= dt;
            }
        }
    }
}