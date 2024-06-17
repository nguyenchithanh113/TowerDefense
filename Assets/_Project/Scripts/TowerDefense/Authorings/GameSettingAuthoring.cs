using TowerDefense.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.Authorings
{
    public class GameSettingAuthoring : MonoBehaviour
    {
        public GameObject normalGunPrefab;
        
    }

    public class GameSettingAuthoringBaker : Baker<GameSettingAuthoring>
    {
        public override void Bake(GameSettingAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            Entity normalGunPrefab = GetEntity(authoring.normalGunPrefab, TransformUsageFlags.Dynamic);

            GameSettingComponent gameSettingComponent = new GameSettingComponent()
            {
                normalGunPrefab = normalGunPrefab,
            };
            
            AddComponent(entity, gameSettingComponent);
        }
    }
}