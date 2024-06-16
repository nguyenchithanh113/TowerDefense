using TowerDefense.Components.Tower;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.Authorings.Tower
{
    public class TowerAuthoring : MonoBehaviour
    {
        
    }

    public class TowerAuthoringBaker : Baker<TowerAuthoring>
    {
        public override void Bake(TowerAuthoring authoring)
        {
            Entity originEntity = GetEntity(TransformUsageFlags.Dynamic);
            
            TowerTag towerTag = new TowerTag();
            
            AddComponent(originEntity, towerTag);
        }
    }
}