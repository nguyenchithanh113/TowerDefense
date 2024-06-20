using Unity.Entities;
using UnityEngine;

namespace TowerDefense.Authorings
{
    public class ShootAuthoring : MonoBehaviour
    {
        public float cooldown = 1;
        
        private class ShootAuthoringBaker : Baker<ShootAuthoring>
        {
            public override void Bake(ShootAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            }
        }
    }
}