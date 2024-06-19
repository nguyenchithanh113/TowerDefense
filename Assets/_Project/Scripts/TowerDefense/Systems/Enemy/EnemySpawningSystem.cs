using TowerDefense.Components;
using TowerDefense.Components.Enemy;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace TowerDefense.Systems.Enemy
{
    public partial struct EnemySpawningSystem : ISystem
    {
        private EntityQuery _enemyQuery;
        private float _timer;
        private float _spawnInterval;
        private int _entityCount;

        private Random _rand;
        
        public void OnCreate(ref SystemState state)
        {
            _rand = new Random((uint)UnityEngine.Random.Range(0, 10000));
            
            state.RequireForUpdate<EnemySetting>();

            _enemyQuery = state.GetEntityQuery(ComponentType.ReadOnly(typeof(EnemyTag)));

            _spawnInterval = 0.02f;
        }

        public void OnUpdate(ref SystemState state)
        {
            EnemySetting enemySetting = SystemAPI.GetSingleton<EnemySetting>();
            
            if(_enemyQuery.CalculateEntityCount() >= 5000) return;

            if (_timer >= _spawnInterval)
            {
                EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

                float2 randomDirection = _rand.NextFloat2() * 2 - new float2(1f, 1f);
                randomDirection = math.normalizesafe(randomDirection);
                float randomRange = _rand.NextFloat(10, 14);
                float2 randPos = randomDirection * randomRange;
                
                Entity spawn = ecb.Instantiate(enemySetting.enemyPrefab);
                ecb.SetComponent(spawn, new LocalTransform()
                {
                    Position = new float3(randPos.x,0,randPos.y),
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
                ecb.SetComponent(spawn, new EnemyTag()
                {
                    index = _entityCount
                });
                
                ecb.Playback(state.EntityManager);
                _timer = 0;
                _entityCount++;
                
                ecb.Dispose();
            }

            _timer += SystemAPI.Time.DeltaTime;
            
        }
    }
}