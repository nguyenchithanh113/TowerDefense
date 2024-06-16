using Unity.Entities;

namespace TowerDefense.Components
{
    public struct TargetEntity : IComponentData
    {
        public Entity value;
    }
}