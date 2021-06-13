using Unity.Entities;
using Unity.NetCode;

namespace Game.Tags
{
    [GenerateAuthoringComponent]
    public struct MovableCubeComponent  : IComponentData
    {
        [GhostField]
        public int ExampleValue;
    }
}