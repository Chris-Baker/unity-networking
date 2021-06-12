using Framework.Networking.Messages;
using Unity.Collections;
using Unity.Entities;

namespace Framework.Networking.Components
{
    // TODO how do we want to use this?
    // TODO we might want to use it like a stack so we can read from the stack as long as we still have more DT to get through
    // TODO and if we dont get through the whole dt (if client ticks at 20Hz vs server 60Hz?) then we leave it there and if we
    // TODO burn through the dt then we remove the input (by moving head backwards)
    // TODO so we treat this as a circular stack
    public struct InputBuffer : IComponentData
    {
        private const int Length = 512;
        private NativeArray<PlayerInputMessage> _inputBuffer;

        public static InputBuffer Create()
        {
            return new InputBuffer()
            {
                _inputBuffer = new NativeArray<PlayerInputMessage>(Length, Allocator.Persistent)
            };
        }

        public void Write(PlayerInputMessage playerInput)
        {
            _inputBuffer[(int) (playerInput.PredictionId % Length)] = playerInput;
        }

        public PlayerInputMessage Read(uint predictionId)
        {
            return _inputBuffer[(int) (predictionId % Length)];
        }
        
        public void Dispose()
        {
            _inputBuffer.Dispose();
        }
    }
}