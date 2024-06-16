using Unity.Entities;

namespace TowerDefense.Components.Projectile
{
    public struct ProjectileSettingComponent : IComponentData
    {
        public Entity normalProjectilePrefab;
    }
}