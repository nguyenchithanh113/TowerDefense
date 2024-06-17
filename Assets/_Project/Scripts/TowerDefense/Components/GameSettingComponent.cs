using Unity.Entities;

namespace TowerDefense.Components
{
    public struct GameSettingComponent  : IComponentData
    {
        public Entity normalGunPrefab;
    }
}