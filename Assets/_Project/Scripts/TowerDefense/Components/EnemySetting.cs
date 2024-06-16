using Unity.Entities;

namespace TowerDefense.Components
{
    public struct EnemySetting : IComponentData
    {
        public Entity enemyPrefab;
    }
}