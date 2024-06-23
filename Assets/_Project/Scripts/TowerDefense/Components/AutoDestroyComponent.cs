using Unity.Entities;

namespace TowerDefense.Components
{
    public struct AutoDestroyComponent : IComponentData
    {
        public float timer;
    }
}