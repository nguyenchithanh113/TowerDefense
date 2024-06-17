using TowerDefense.Components;
using TowerDefense.Components.Enemy;
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
        private EntityQuery _towerEntityQuery;

        public void OnCreate(ref SystemState state)
        {
            var enemyEntityQueryDes = new EntityQueryDesc()
            {
                None = new[] { ComponentType.ReadWrite(typeof(TargetEntity)) },
                All = new[] { ComponentType.ReadOnly<EnemyTag>() , ComponentType.ReadOnly<LocalTransform>()}
            };

            _enemyEntityQuery = state.GetEntityQuery(enemyEntityQueryDes);
            _towerEntityQuery = state.GetEntityQuery(TowerQueryDesc.TowerWithTransformDescRO());
        }

        public void OnUpdate(ref SystemState state)
        {
            var dependency = state.Dependency;

            int towerCount = _towerEntityQuery.CalculateEntityCount();
            int enemyCount = _enemyEntityQuery.CalculateEntityCount();

            if (towerCount == 0 || enemyCount == 0) return;

            NativeArray<Entity> towerArr = new NativeArray<Entity>(towerCount, Allocator.TempJob);
            NativeArray<LocalTransform> towerTransformArr =
                new NativeArray<LocalTransform>(towerCount, Allocator.TempJob);

            NativeArray<Entity> enemyArr = new NativeArray<Entity>(towerCount, Allocator.TempJob);
            NativeArray<LocalTransform> enemyTransformArr =
                new NativeArray<LocalTransform>(towerCount, Allocator.TempJob);

            var towerJobHandle = new GetEntityAndTransform()
            {
                entities = towerArr,
                transforms = towerTransformArr
            }.ScheduleParallel(_towerEntityQuery, dependency);

            var entityJobHandle = new GetEntityAndTransform()
            {
                entities = enemyArr,
                transforms = enemyTransformArr
            }.ScheduleParallel(_enemyEntityQuery, dependency);

            dependency = JobHandle.CombineDependencies(towerJobHandle, entityJobHandle);
            dependency.Complete();
            state.Dependency = dependency;

            CheckTargetForEnemy(ref state, enemyArr);
            AddingAttackTargetForEnemy(ref state, towerArr, towerTransformArr, enemyArr, enemyTransformArr);
            
            towerArr.Dispose();
            towerTransformArr.Dispose();
            enemyArr.Dispose();
            enemyTransformArr.Dispose();
        }

        void CheckTargetForEnemy(ref SystemState state, NativeArray<Entity> enemyArr)
        {
            var dependency = state.Dependency;

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            dependency = new CheckIfEnemyTargetIsValidJob()
            {
                targetEntityLookUp = state.GetComponentLookup<TargetEntity>(),
                towerTagLookup = state.GetComponentLookup<TowerTag>(),
                enemyArr = enemyArr,
                commandBuffer = ecb.AsParallelWriter()
            }.ScheduleParallel(enemyArr.Length, 64, dependency);
            
            dependency.Complete();
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            state.Dependency = dependency;
        }

        void AddingAttackTargetForEnemy(ref SystemState state, NativeArray<Entity> towerArr,
            NativeArray<LocalTransform> towerTransformArr, NativeArray<Entity> enemyArr,
            NativeArray<LocalTransform> enemyTransformArr)
        {
            var dependency = state.Dependency;

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            dependency = new GetNearestTowerTargetForEnemyJob()
            {
                towers = towerArr,
                towerTransforms = towerTransformArr,
                enemies = enemyArr,
                enemyTransforms = enemyTransformArr,
                ecbParallelWriter = ecb.AsParallelWriter()
            }.ScheduleParallel(enemyArr.Length, 64, dependency);

            dependency.Complete();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            state.Dependency = dependency;
        }
    }

    [BurstCompile]
    public partial struct GetEntityAndTransform : IJobEntity
    {
        [NativeDisableParallelForRestriction] public NativeArray<Entity> entities;
        [NativeDisableParallelForRestriction] public NativeArray<LocalTransform> transforms;

        public void Execute([EntityIndexInQuery] int index, Entity entity, in LocalTransform transform)
        {
            entities[index] = entity;
            transforms[index] = transform;
        }
    }

    [BurstCompile]
    public struct CheckIfEnemyTargetIsValidJob : IJobFor
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<TargetEntity> targetEntityLookUp;

        [NativeDisableParallelForRestriction] public ComponentLookup<TowerTag> towerTagLookup;

        public EntityCommandBuffer.ParallelWriter commandBuffer;

        public NativeArray<Entity> enemyArr;

        public void Execute(int index)
        {
            var enemy = enemyArr[index];

            if (targetEntityLookUp.HasComponent(enemy))
            {
                var enemyTarget = targetEntityLookUp.GetRefRO(enemy);

                if (!towerTagLookup.HasComponent(enemyTarget.ValueRO.value))
                {
                    commandBuffer.RemoveComponent<TargetEntity>(index,enemy);
                }
            }
        }
    }

    [BurstCompile]
    public struct GetNearestTowerTargetForEnemyJob : IJobFor
    {
        [ReadOnly] public NativeArray<Entity> towers;
        [ReadOnly] public NativeArray<LocalTransform> towerTransforms;
        public EntityCommandBuffer.ParallelWriter ecbParallelWriter;

        [NativeDisableParallelForRestriction] public NativeArray<Entity> enemies;
        [NativeDisableParallelForRestriction] public NativeArray<LocalTransform> enemyTransforms;

        public void Execute(int index)
        {
            var enemy = enemies[index];
            var enemyTransform = enemyTransforms[index];
            var enemyPosition = enemyTransform.Position;

            int towerTargetIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < towerTransforms.Length; i++)
            {
                var towerPos = towerTransforms[i].Position;

                var sqrMag = math.distancesq(towerPos, enemyPosition);
                if (sqrMag < minDistance)
                {
                    minDistance = sqrMag;
                    towerTargetIndex = i;
                }
            }

            TargetEntity targetEntity = new TargetEntity()
            {
                value = towers[towerTargetIndex]
            };
            ecbParallelWriter.AddComponent(index, enemy, targetEntity);
        }
    }
}