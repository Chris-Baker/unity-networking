using System; 
using Unity.Entities;
using UnityEngine;

namespace Framework.Networking.Components
{
    [Serializable]
    public struct PlayerSlot : IComponentData
    {
        [HideInInspector] public int slot;
    }
}
