using EcsExtension;
using ProjectDawn.Mathematics;
using TowerDefense.Components.Player;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TowerDefense.Systems.Player
{
    public partial struct PlayerRotateSystem : ISystem
    {
        private EntityQuery _playerTurretQuery;
        
        public void OnCreate(ref SystemState state)
        {
            _playerTurretQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new []
                {
                    ComponentType.ReadOnly<PlayerTag>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadWrite<LocalTransform>()
                }
            });
        }
        
        public void OnUpdate(ref SystemState state)
        {
            JobHandle dependency = state.Dependency; 
            
            float2 joystickDirection = InputSystemCapture.JoystickDirection.asfloat();
            
            if(joystickDirection.Equals(float2.zero)) return;

            new PlayerTurretRotateJob()
            {
                dt = SystemAPI.Time.DeltaTime,
                rotateDirection = joystickDirection,
            }.Schedule(_playerTurretQuery,dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }

    public partial struct PlayerTurretRotateJob : IJobEntity
    {
        public float2 rotateDirection;
        public float dt;
        
        public void Execute(ref LocalTransform localTransform)
        {
            //math.sign()
            quaternion from = localTransform.Rotation;
            quaternion to = quaternion.LookRotation(new float3(rotateDirection.x, 0, rotateDirection.y),
                new float3(1f, 1f, 1f));

            quaternion targetRotate = MathHelper.RotateTowards(from, to, 90 * dt);
            localTransform.Rotation = targetRotate;
        }
    }
}