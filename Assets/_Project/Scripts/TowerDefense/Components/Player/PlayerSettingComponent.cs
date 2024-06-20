using Unity.Entities;

namespace TowerDefense.Components.Player
{
    public struct PlayerSettingComponent : IComponentData
    {
        public Entity projectilePrefab;
    }
}