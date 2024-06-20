using Unity.Entities;

namespace TowerDefense.Components
{
    public struct Shoot : IComponentData, IEnableableComponent
    {
        public float cooldown;
        public float cooldownTimer;
    }
}