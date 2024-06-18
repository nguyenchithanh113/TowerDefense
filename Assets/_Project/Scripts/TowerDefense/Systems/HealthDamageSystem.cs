using TowerDefense.Components;
using Unity.Entities;
using Unity.Jobs;

namespace TowerDefense.Systems
{
    public partial struct HealthDamageSystem : ISystem
    {
        private EntityQuery _healthDamageQuery;

        public void OnCreate(ref SystemState state)
        {
            _healthDamageQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadWrite<HealthComponent>(),
                    ComponentType.ReadWrite<DamageBufferElement>(),
                }
            });
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new HealthDamageCalculationJob()
            {

            }.ScheduleParallel(_healthDamageQuery, state.Dependency);
        }
    }

    public partial struct HealthDamageCalculationJob : IJobEntity
    {
        public void Execute(Entity entity, ref HealthComponent healthComponent,
            ref DynamicBuffer<DamageBufferElement> damageBuffer)
        {
            for (int i = 0; i < damageBuffer.Length; i++)
            {
                healthComponent.value -= damageBuffer[i].value;
            }
            
            damageBuffer.Clear();
        }
    }
}