using TowerDefense.Components.Projectile;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.Authorings.Projectile
{
    public class ProjectileComponentAuthoring : MonoBehaviour
    {
        public float speed;
        public float damage;
    }

    public class ProjectileComponentAuthoringBaker : Baker<ProjectileComponentAuthoring>
    {
        public override void Bake(ProjectileComponentAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            ProjectileComponent projectileComponent = new ProjectileComponent()
            {
                speed = authoring.speed,
                damage = authoring.damage
            };

            AddComponent(entity, projectileComponent);
        }
    }
}