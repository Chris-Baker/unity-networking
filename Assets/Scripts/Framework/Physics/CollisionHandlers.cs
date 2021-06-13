using System;
using System.Collections.Generic;
using Framework.Physics.Components;
using Game.Components;
using Unity.Entities;
using UnityEngine.XR;

namespace Framework.Physics
{
    public static class CollisionHandlers
    {
        // TODO dynamic type here should be T where T : struct, ICollision but /shrug 
        private static readonly Dictionary<int, Action<Collision, dynamic, ComponentDataFromEntity<PhysicsLayer>>> Handlers 
            = new Dictionary<int, Action<Collision, dynamic, ComponentDataFromEntity<PhysicsLayer>>>();

        public static void AddHandler(int key, Action<Collision, dynamic, ComponentDataFromEntity<PhysicsLayer>> action)
        {
            if (Handlers.ContainsKey(key))
            {
                Handlers[key] = action;
                return;
            }
            Handlers.Add(key, action);
        }

        public static Action<Collision, dynamic, ComponentDataFromEntity<PhysicsLayer>> GetHandler(int key)
        {
            return Handlers[key];
        }

        public static void ClearHandlers()
        {
            Handlers.Clear();
        }

        public static bool Exists(int key)
        {
            return Handlers.ContainsKey(key);
        }
    }
}