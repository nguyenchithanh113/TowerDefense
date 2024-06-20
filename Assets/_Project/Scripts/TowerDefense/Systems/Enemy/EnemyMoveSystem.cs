using TowerDefense.Components;
using TowerDefense.Components.Enemy;
using TowerDefense.Components.Tower;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// namespace TowerDefense.Systems.Enemy
// {
//     public partial struct EnemyMoveSystem : ISystem
//     {
//         private EntityQuery _enemyQuery;
//
//         public void OnCreate(ref SystemState state)
//         {
//             _enemyQuery = state.GetEntityQuery(new[]
//             {
//                 ComponentType.ReadOnly<EnemyTag>(),
//                 ComponentType.ReadOnly<Speed>(),
//                 ComponentType.ReadWrite<LocalTransform>(),
//                 ComponentType.ReadOnly<TargetEntity>()
//             });
//         }
//
//         [BurstCompile]
//         public void OnUpdate(ref SystemState state)
//         {
//             state.Dependency = new MoveToTargetJob()
//             {
//                 transformLookUp = state.GetComponentLookup<LocalTransform>(),
//                 dt = SystemAPI.Time.DeltaTime,
//             }.ScheduleParallel(_enemyQuery, state.Dependency);
//         }
//         
//         [BurstCompile]
//         public partial struct MoveToTargetJob : IJobEntity
//         {
//             [NativeDisableParallelForRestriction]
//             public ComponentLookup<LocalTransform> transformLookUp;
//
//             public float dt;
//
//             public void Execute(Entity entity, in TargetEntity targetEntity, in Speed speed)
//             {
//                 if (transformLookUp.HasComponent(targetEntity.value) && transformLookUp.HasComponent(entity))
//                 {
//                     ref LocalTransform transform = ref transformLookUp.GetRefRW(entity).ValueRW;
//                     var targetTransform = transformLookUp.GetRefRO(targetEntity.value).ValueRO;
//                     float distanceSq = math.distancesq(transform.Position, targetTransform.Position);
//
//                     if (distanceSq > 1)
//                     {
//                         float3 direction = math.normalizesafe((targetTransform.Position - transform.Position));
//
//                         transform.Position += direction * speed.value * dt;
//                     }
//                     
//                 }
//             }
//         }
//     }
// }