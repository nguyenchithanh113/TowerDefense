using TowerDefense.Components.Gun;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.Authorings.Gun
{
    public class NormalGunAuthoring : MonoBehaviour
    {
        public GameObject projectile;

        public float cooldown = 1f;
    }

    public class NormalGunAuthoringBaker : Baker<NormalGunAuthoring>
    {
        public override void Bake(NormalGunAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            GunComponent gunComponent = new GunComponent()
            {
                projectilePrefab = GetEntity(authoring.projectile, TransformUsageFlags.Dynamic),
                cooldown = authoring.cooldown,
                cooldownTimer = 0,
            };
            
            AddComponent(entity, gunComponent);
        }
    }
}