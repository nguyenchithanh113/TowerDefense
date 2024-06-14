using TowerDefense.Components.Enemy;
using TowerDefense.Components.Tower;
using Unity.Entities;
using Unity.Transforms;

namespace TowerDefense.Systems.Enemy
{
    public partial struct EnemyMoveSystem : ISystem
    {
        private EntityQuery _enemyEntityQuery;
        private EntityQuery _towerEntityQuery;

        public void OnCreate(ref SystemState state)
        {
            _enemyEntityQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<TowerTag>(),
                ComponentType.ReadOnly<LocalTransform>());
            _towerEntityQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<EnemyTag>(),
                typeof(LocalTransform));
        }

        public void OnUpdate(ref SystemState state)
        {
            
        }
    }
}