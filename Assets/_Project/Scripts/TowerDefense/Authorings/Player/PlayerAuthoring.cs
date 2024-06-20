using TowerDefense.Components;
using TowerDefense.Components.Player;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.Authorings.Player
{
    public class PlayerAuthoring : MonoBehaviour
    {
        public float shootCooldown = 0.15f;
        
        private class PlayerAuthoringBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent<PlayerTag>(entity);
                AddComponent(entity, new Shoot()
                {
                    cooldown = authoring.shootCooldown
                });
            }
        }
    }
}