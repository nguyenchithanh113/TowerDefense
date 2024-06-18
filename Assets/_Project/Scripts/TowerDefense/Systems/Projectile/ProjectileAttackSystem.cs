using TowerDefense.Components.Projectile;
using Unity.Entities;
using Unity.Transforms;

namespace TowerDefense.Systems.Projectile
{
    public partial struct ProjectileAttackSystem : ISystem
    {
        private EntityQuery _projectileQuery;

        public void OnCreate(ref SystemState state)
        {
            _projectileQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadWrite<ProjectileComponent>(),
                    ComponentType.ReadWrite<LocalTransform>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                }
            });
        }

        public void OnUpdate(ref SystemState state)
        {
            
        }
        
        public partial struct ProjectileMoveToTargetJob : IJobEntity
        {
            public void Execute(in ProjectileComponent projectileComponent)
            {
                
            }
        }
    }
}