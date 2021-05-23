using Unity.Entities;
using Unity.Transforms;

namespace Game.Events.Data
{
    [GenerateAuthoringComponent]
    public struct Spawn : IComponentData
    {
        public Translation Translation;
    }
}