using Framework.Events;
using Game.Components;
using Game.Events.Data;
using Unity.Entities;
using Unity.Jobs;

namespace Game.Events.Handlers
{
    [UpdateInGroup(typeof(EventGroup))]
    public class PlayerSpawnEventHandler: JobComponentSystem
    {
        private BeginInitializationEntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // we need a command buffer so we can create events at the next sync point on the main thread
            // we are using the begin initialisation command buffer so this runs at the start of the next frame
            EntityCommandBuffer.ParallelWriter commandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            
            // get our prefabs references
            Prefabs prefabs = GetSingleton<Prefabs>();
            
            // ref to read write, in to read
            JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, in PlayerSpawnEvent playerSpawnEvent) =>
            {
                // create a bullet entity
                Entity player = commandBuffer.Instantiate(entityInQueryIndex, prefabs.PlayerPrefab);
                
                // set our position
                commandBuffer.SetComponent(entityInQueryIndex, player, playerSpawnEvent.Translation);
                
                // TODO spawn a player renderer prefab from a resource link
                
            }).Schedule(inputDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
            
            return jobHandle;
        }
    }
}