using Unity.Entities;
using Unity.Mathematics;

namespace TowerDefense.Components.Projectile
{
    public struct ProjectileComponent : IComponentData
    {
        public float speed;
        public float damage;

        public float3 lastPosition;

        public Entity explodeParticlePrefab;
    }
}