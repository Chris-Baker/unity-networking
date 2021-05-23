using Unity.Entities;
using UnityEngine;

namespace Framework.Networking.Components
{
    [GenerateAuthoringComponent]
    public struct PlayerInput : IComponentData
    {
        [HideInInspector] public bool Up, Down, Left, Right, Shoot, Use, Sprint, Jump;
        [HideInInspector] public float Yaw, Pitch;
        [HideInInspector] public uint PredictionId;
    }
}
