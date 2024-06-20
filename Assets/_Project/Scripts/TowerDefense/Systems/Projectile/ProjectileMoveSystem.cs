using Unity.Burst;
using Unity.Entities;

namespace TowerDefense.Systems.Projectile
{
    public partial struct ProjectileMoveSystem : ISystem
    {

        public void OnCreate(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {

        }
        
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}