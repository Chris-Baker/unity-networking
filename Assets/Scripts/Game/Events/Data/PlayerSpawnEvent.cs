using Unity.Entities;
using Unity.Transforms;

namespace Game.Events.Data
{
    [GenerateAuthoringComponent]
    public struct PlayerSpawnEvent : IComponentData
    {
        public Translation Translation;
    }
}