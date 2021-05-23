using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Framework.Networking.Components
{
    [Serializable]
    public struct PlayerInfo : IComponentData
    {
        [HideInInspector] public FixedString128 name;
    }
}
