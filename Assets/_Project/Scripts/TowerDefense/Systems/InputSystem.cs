using EcsExtension;
using Jobs;
using TowerDefense.Components.Enemy;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace TowerDefense.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(BeginSimulationEntityCommandBufferSystem))]
    public partial struct InputSystem : ISystem
    {
        public bool IsMouseDown;
        public Vector2 MousePosition;

        public void OnCreate(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {
            IsMouseDown = InputSystemCapture.IsMouseDown;
            MousePosition = InputSystemCapture.MousePosition;
        }
    }
}