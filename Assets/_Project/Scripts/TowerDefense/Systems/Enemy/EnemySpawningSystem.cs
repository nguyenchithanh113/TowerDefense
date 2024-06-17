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
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemySetting>();

            _enemyQuery = state.GetEntityQuery(ComponentType.ReadOnly(typeof(EnemyTag)));

            _spawnInterval = 0.05f;
        }

        public void OnUpdate(ref SystemState state)
        {
            EnemySetting enemySetting = SystemAPI.GetSingleton<EnemySetting>();
            
            if(_enemyQuery.CalculateEntityCount() >= 10) return;

            if (_timer >= _spawnInterval)
            {
                EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
                
                Entity spawn = ecb.Instantiate(enemySetting.enemyPrefab);
                ecb.SetComponent(spawn, new LocalTransform()
                {
                    Position = new float3(0,0,10),
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