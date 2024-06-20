using TowerDefense.Components;
using TowerDefense.Components.Enemy;
using TowerDefense.Components.Player;
using TowerDefense.Components.Tower;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace TowerDefense.Systems.Enemy
{
    public partial struct EnemyTargetToAttackSystem : ISystem
    {
        private EntityQuery _enemyEntityQuery;
        private EntityQuery _playerEntityQuery;

        public void OnCreate(ref SystemState state)
        {
            var enemyEntityQueryDes = new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<EnemyTag>(),
                    ComponentType.ReadWrite<LocalTransform>()
                }
            };

            _enemyEntityQuery = state.GetEntityQuery(enemyEntityQueryDes);
            _playerEntityQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new []
                {
                    ComponentType.ReadOnly<PlayerTag>(), 
                    ComponentType.ReadOnly<LocalTransform>(), 
                }
            });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dependency = state.Dependency;

            int playerCount = _playerEntityQuery.CalculateEntityCount();
            int enemyCount = _enemyEntityQuery.CalculateEntityCount();

            if (playerCount == 0 || enemyCount == 0) return;

            LocalTransform playerLocalTransform = _playerEntityQuery.GetSingleton<LocalTransform>();

            state.Dependency = new MoveToTargetJob()
            {
                targetPos = playerLocalTransform.Position,
                moveSpeed = 2,
                dt = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(_enemyEntityQuery, state.Dependency);
        }
    }

    // public struct GetEntityLocalTransformJob : IJob
    // {
    //     public ComponentLookup<LocalTransform> localTransformLookup;
    //     public LocalTransform localTransformArr;
    //     public void Execute()
    //     {
    //         
    //     }
    // }
    
    public partial struct MoveToTargetJob : IJobEntity
    {
        public float3 targetPos;

        public float moveSpeed;
        public float dt;

        public void Execute(ref LocalTransform localTransform)
        {
            float3 direction = math.normalizesafe(targetPos - localTransform.Position);

            localTransform.Position += direction * moveSpeed * dt;
        }
    }

    
}