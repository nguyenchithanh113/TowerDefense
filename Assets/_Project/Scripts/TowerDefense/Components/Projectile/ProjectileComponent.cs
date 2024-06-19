using Unity.Entities;

namespace TowerDefense.Components.Projectile
{
    public struct ProjectileComponent : IComponentData
    {
        public float speed;
        public float damage;

        public Entity explodeParticlePrefab;
    }
}