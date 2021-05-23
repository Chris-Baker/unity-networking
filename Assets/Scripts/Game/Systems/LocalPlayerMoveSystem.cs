using Game.Components;
using Game.Tags;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.InputSystem;

namespace Game.Systems
{
    public class LocalPlayerMoveSystem : JobComponentSystem
    {
        protected override void OnCreate() {
            RequireSingletonForUpdate<DisabledSystem>();
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // get the keyboard
            Keyboard keyboard = Keyboard.current;

            // get the normalised input direction
            float2 direction = new float2();

            if (keyboard[Key.D].isPressed)
            {
                direction.x += 1;
            }
            if (keyboard[Key.A].isPressed)
            {
                direction.x -= 1;
            }
            if (keyboard[Key.W].isPressed)
            {
                direction.y += 1;
            }
            if (keyboard[Key.S].isPressed)
            {
                direction.y -= 1;
            }
            
            direction = math.normalizesafe(direction);
            
            // Debug.Log(direction.x + ", " + direction.y);
            
            // ref to read write, in to read
            JobHandle jobHandle = Entities.ForEach((ref MoveDirection moveDirection, in Rotation rotation) =>
            {
                moveDirection.Direction.x = direction.x;
                moveDirection.Direction.z = direction.y;
                moveDirection.Direction = math.rotate(math.inverse(rotation.Value), moveDirection.Direction);
            }).Schedule(inputDeps);

            return jobHandle;
        }
    }
}
