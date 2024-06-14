using TowerDefense.Components;
using TowerDefense.Components.Enemy;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.Authorings.Enemy
{
    public class EnemyAuthoring : MonoBehaviour
    {
        public float moveSpeed;
    }

    public class EnemyAuthoringBaker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity originEntity = GetEntity(TransformUsageFlags.Dynamic);
            
            EnemyTag enemyTag = new EnemyTag();
            
            Speed speed = new Speed() { value = authoring.moveSpeed };
            
            AddComponent(originEntity, enemyTag);
            AddComponent(originEntity, speed);
        }
    }
}