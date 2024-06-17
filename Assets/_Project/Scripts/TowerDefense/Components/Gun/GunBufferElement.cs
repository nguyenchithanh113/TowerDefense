using Unity.Entities;

namespace TowerDefense.Components.Gun
{
    public struct GunBufferElement : IBufferElementData
    {
        public Entity entity;
    }
}