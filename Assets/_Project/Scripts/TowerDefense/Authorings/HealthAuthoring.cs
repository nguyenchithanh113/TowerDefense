using TowerDefense.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.Authorings
{
    public class HealthAuthoring : MonoBehaviour
    {
        public float value = 100;
    }

    public class HealthAuthoringBaker : Baker<HealthAuthoring>
    {
        public override void Bake(HealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            HealthComponent healthComponent = new HealthComponent()
            {
                value = authoring.value
            };
    
            AddBuffer<DamageBufferElement>(entity);
            AddComponent<HealthComponent>(entity, healthComponent);
        }
    }
}