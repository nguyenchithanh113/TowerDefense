using Unity.Entities;

namespace TowerDefense.Components.Gun
{
    public struct GunComponent : IComponentData
    {
        public Entity projectilePrefab;

        public float cooldown;
        public float cooldownTimer;
    }
}