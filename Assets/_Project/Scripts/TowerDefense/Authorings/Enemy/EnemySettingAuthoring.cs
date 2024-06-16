using TowerDefense.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.Authorings.Enemy
{
    public class EnemySettingAuthoring : MonoBehaviour
    {
        public GameObject enemyPrefab;
    }

    public class EnemySettingAuthoringBaker : Baker<EnemySettingAuthoring>
    {
        public override void Bake(EnemySettingAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            EnemySetting enemySetting = new EnemySetting()
            {
                enemyPrefab = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic)
            };
            
            AddComponent(entity, enemySetting);
        }
    }
}