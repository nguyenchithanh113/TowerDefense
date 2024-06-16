using Unity.Entities;
using Unity.Transforms;

namespace TowerDefense.Components.Tower
{
    public static class TowerQueryDesc
    {
        public static EntityQueryDesc TowerWithTransformDescRW()
        {
            return new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<TowerTag>(),
                    ComponentType.ReadWrite<LocalTransform>()
                }
            };
        }
        
        public static EntityQueryDesc TowerWithTransformDescRO()
        {
            return new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<TowerTag>(),
                    ComponentType.ReadOnly<LocalTransform>()
                }
            };
        }
        
    }
}