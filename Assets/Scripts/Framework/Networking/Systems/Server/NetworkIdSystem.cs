using Game.Tags;
using Unity.Entities;

namespace Framework.Networking.Systems.Server
{
    public class NetworkIdSystem : ComponentSystem
    {
        private uint _nextId;
        
        protected override void OnCreate() {
            _nextId = 0;
            RequireSingletonForUpdate<DisabledSystem>();
        }
        
        protected override void OnUpdate() {}

        public uint NextId()
        {
            uint id = _nextId;
            _nextId += 1;
            return id;
        }
    }
}