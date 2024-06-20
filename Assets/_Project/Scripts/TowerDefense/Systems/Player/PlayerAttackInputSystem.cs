using TowerDefense.Components;
using TowerDefense.Components.Player;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace TowerDefense.Systems.Player
{
    public partial struct PlayerAttackInputSystem : ISystem, ISystemStartStop
    {
        private EntityQuery _playerTagQuery;
        
        public void OnCreate(ref SystemState state)
        {
            _playerTagQuery = state.GetEntityQuery(new EntityQueryDesc()
            {
                Present = new[]
                {
                    ComponentType.ReadOnly<PlayerTag>(),
                    ComponentType.ReadOnly<Shoot>(),
                }
            });
            
        }
        
        public void OnStartRunning(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            state.Dependency = new PlayerRemoveShootJob()
            {
                ecb = ecb
            }.Schedule(_playerTagQuery, state.Dependency);
            state.Dependency.Complete();
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            bool isMouseDown = InputSystemCapture.IsMouseDown;
            bool isMouseUp = InputSystemCapture.IsMouseUp;

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            if (isMouseDown)
            {
                state.Dependency = new PlayerAddShootJob()
                {
                    ecb = ecb
                }.Schedule(_playerTagQuery, state.Dependency);
                state.Dependency.Complete();
            }

            if (isMouseUp)
            {
                state.Dependency = new PlayerRemoveShootJob()
                {
                    ecb = ecb
                }.Schedule(_playerTagQuery, state.Dependency);
                state.Dependency.Complete();
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        

        public void OnStopRunning(ref SystemState state)
        {
        }
    }
    
    [BurstCompile]
    public partial struct PlayerAddShootJob : IJobEntity
    {
        public EntityCommandBuffer ecb;

        public void Execute(Entity entity)
        {
            ecb.SetComponentEnabled<Shoot>(entity, true);
        }
    }

    [BurstCompile]
    public partial struct PlayerRemoveShootJob : IJobEntity
    {
        public EntityCommandBuffer ecb;
        
        public void Execute(Entity entity)
        {
            ecb.SetComponentEnabled<Shoot>(entity, false);
        }
    }
}