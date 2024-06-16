using TowerDefense.Components.Tower;
using Unity.Entities;

namespace TowerDefense.Systems
{
    public partial struct TowerShootSystem : ISystem, ISystemStartStop
    {
        private EntityQuery _towerShotQuery;
        private EntityQuery _towerQuery;
        
        public void OnCreate(ref SystemState state)
        {
            _towerShotQuery = state.GetEntityQuery(new[]
            {
                ComponentType.ReadOnly<TowerShootSystem>(),
            });

            _towerQuery = state.GetEntityQuery(new[]
            {
                ComponentType.ReadOnly<TowerTag>(),
            });
        }

        public void OnStartRunning(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {
            
        }

        public void OnStopRunning(ref SystemState state)
        {
            
        }
    }
}