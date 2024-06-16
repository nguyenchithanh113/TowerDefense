using Unity.Entities;

namespace TowerDefense.Components
{
    public struct GunComponent : IComponentData
    {
        public Entity projectilePrefab;
    }
}