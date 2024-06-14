using TowerDefense.Components;
using TowerDefense.Components.Enemy;
using TowerDefense.Components.Tower;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace TowerDefense.Systems.Enemy
{
    public partial struct EnemyTargetToAttackSystem : ISystem
    {
        private EntityQuery _enemyEntityQuery;
        private EntityQuery _towerEntityQuery;
        
        public void OnCreate(ref SystemState state)
        {
            var enemyEntityQueryDes = new EntityQueryDesc()
            {
                None = new[] { ComponentType.ReadWrite(typeof(TargetEntity)) },
                All = new[] { ComponentType.ReadOnly<EnemyTag>() }
            };

            _enemyEntityQuery = state.GetEntityQuery(enemyEntityQueryDes);
            _towerEntityQuery = state.GetEntityQuery(TowerQueryDesc.TowerWithTransformDescRO());
        }

        public void OnUpdate(ref SystemState state)
        {
            var dependency = state.Dependency;
            
            int towerCount = _towerEntityQuery.CalculateEntityCount();
            
            if(towerCount == 0) return;

            NativeArray<Entity> towerArr = new NativeArray<Entity>(towerCount, Allocator.TempJob);
            NativeArray<LocalTransform> towerTransformArr = new NativeArray<LocalTransform>(towerCount, Allocator.TempJob);
            
            NativeArray<Entity> enemyArr = new NativeArray<Entity>(towerCount, Allocator.TempJob);
            NativeArray<LocalTransform> enemyTransformArr = new NativeArray<LocalTransform>(towerCount, Allocator.TempJob);

            var towerJobHandle = new GetEntityAndTransform()
            {
                entities = towerArr,
                transforms = towerTransformArr
            }.ScheduleParallel(_towerEntityQuery, dependency);
            
            var entityJobHandle = new GetEntityAndTransform()
            {
                entities = enemyArr,
                transforms = enemyTransformArr
            }.ScheduleParallel(_towerEntityQuery, dependency);
            
            dependency = JobHandle.CombineDependencies(towerJobHandle, entityJobHandle);
            
            
        }
    }
    
    public partial struct GetEntityAndTransform : IJobEntity
    {
        [NativeDisableParallelForRestriction] 
        public NativeArray<Entity> entities;
        [NativeDisableParallelForRestriction]
        public NativeArray<LocalTransform> transforms;
        public void Execute([EntityIndexInQuery] int index, Entity entity, in LocalTransform transform)
        {
            entities[index] = entity;
            transforms[index] = transform;
        }
    }
    
    public struct GetNearestTowerForEnemyJob : IJobFor
    {
        [ReadOnly] public NativeArray<Entity> towers;
        [ReadOnly] public NativeArray<LocalTransform> towerTransforms;

        public NativeArray<Entity> enemies;
        public NativeArray<LocalTransform> enemyTransforms;
        
        public void Execute(int index)
        {
            var enemy = enemies[index];
            var enemyTransform = enemyTransforms[index];
            var enemyPosition = enemyTransform.Position;

            int towerTargetIndex = 0;
            float minDistance = 0
        }
    }
}