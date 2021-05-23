using Unity.Entities;
using Unity.Jobs;
using UnityEngine.InputSystem;
using PlayerInput = Framework.Networking.Components.PlayerInput;

namespace Framework.Networking.Systems.Client
{
    public class PlayerInputSystem : JobComponentSystem
    {
        private uint _predictionId = 0;
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            _predictionId += 1 % uint.MaxValue;
            
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            bool up = keyboard[Key.W].isPressed;
            bool down = keyboard[Key.S].isPressed;
            bool left = keyboard[Key.A].isPressed;
            bool right = keyboard[Key.D].isPressed;
            bool shoot = mouse.leftButton.isPressed;
            bool use = keyboard[Key.E].isPressed;
            bool sprint = keyboard[Key.LeftShift].isPressed;
            bool jump = keyboard[Key.Space].isPressed;

            uint predictedId = _predictionId;
            
            // ref to read write, in to read
            JobHandle jobHandle = Entities.ForEach((ref PlayerInput input) =>
            {
                input.Up = up;
                input.Down = down;
                input.Left = left;
                input.Right = right;
                input.Shoot = shoot;
                input.Use = use;
                input.Sprint = sprint;
                input.Jump = jump;
                input.PredictionId = predictedId;
            }).Schedule(inputDeps);

            return jobHandle;
        }
    }
}
