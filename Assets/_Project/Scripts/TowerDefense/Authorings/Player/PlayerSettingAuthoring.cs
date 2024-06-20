using TowerDefense.Components.Player;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.Authorings.Player
{
    public class PlayerSettingAuthoring : MonoBehaviour
    {
        public GameObject projectilePrefab;
        private class PlayerSettingAuthoringBaker : Baker<PlayerSettingAuthoring>
        {
            public override void Bake(PlayerSettingAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                
                AddComponent<PlayerSettingComponent>(entity, new PlayerSettingComponent()
                {
                    projectilePrefab = GetEntity(authoring.projectilePrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}