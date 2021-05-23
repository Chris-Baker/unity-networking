using System;
using Unity.Entities;
using UnityEngine;

namespace Framework.Networking.Components
{
    [Serializable]
    public struct Replicated : IComponentData
    {
        [HideInInspector] public int id;
    }
}
