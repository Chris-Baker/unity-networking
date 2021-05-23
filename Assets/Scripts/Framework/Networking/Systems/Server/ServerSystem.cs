using Framework.Networking.Messages;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Assertions;

namespace Framework.Networking.Systems.Server
{
    class ServerSystem : ComponentSystem {
        private NetworkDriver _driver;
        private NativeList<NetworkConnection> _connections;

        // per connection input buffer
        private NativeList<InputBuffer> _inputBuffers;
        
        protected override void OnCreate() {
            RequireSingletonForUpdate<Tags.Server>();
        }

        protected override void OnStartRunning()
        {
            _driver = NetworkDriver.Create();
            NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
            endpoint.Port = 9000;
            if (_driver.Bind(endpoint) != 0)
            {
                Debug.Log("Failed to bind to port 9000");
            }
            else
            {
                _driver.Listen();
            }
            _connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
            _inputBuffers = new NativeList<InputBuffer>(16, Allocator.Persistent);
        }

        protected override void OnUpdate()
        {
            _driver.ScheduleUpdate().Complete();
            CleanupConnections();
            AcceptNewConnections();

            DataStreamReader stream;
            for (int slot = 0; slot < _connections.Length; slot++)
            {
                Assert.IsTrue(_connections[slot].IsCreated);

                NetworkEvent.Type cmd;
                while ((cmd = _driver.PopEventForConnection(_connections[slot], out stream)) != NetworkEvent.Type.Empty)
                {
                    switch (cmd)
                    {
                        case NetworkEvent.Type.Data:
                        {
                            byte messageType = stream.ReadByte();

                            switch (messageType)
                            {
                                case MessageType.ConnectionInfo:
                                    ConnectionInfoMessage request = ConnectionInfoMessage.Deserialize(stream);
                                    ConnectionResultMessage response = new ConnectionResultMessage()
                                    {
                                        Message = request.Name + " Joined"
                                    };
                                    
                                    _driver.BeginSend(NetworkPipeline.Null, _connections[slot], out DataStreamWriter writer);
                                    response.Serialize(ref writer);
                                    _driver.EndSend(writer);
                                    break;
                                
                                case MessageType.PlayerInput:
                                    Debug.Log(PlayerInputMessage.Deserialize(stream).Up);
                                    break;
                            }
                            
                            break;
                        }
                        case NetworkEvent.Type.Disconnect:
                            Debug.Log("Client disconnected from server");
                            _connections[slot] = default(NetworkConnection);
                            break;
                    }
                }
            }
            
            // Create player and assign network id
            // Tell other players to do the same
            // Send list of all objects to create to new player
            // Send updated snapshot data for each object
            
            // Handle input streams
            // Get inputs for each player
            // Update simulation
            // Send snapshots
            
            // Handle RPCs
            
            // Update local simulation
            // Create a bunch of snapshots
        }
        
        private void CleanupConnections()
        {
            for (int slot = 0; slot < _connections.Length; slot++)
            {
                if (_connections[slot].IsCreated) continue;
                _connections.RemoveAtSwapBack(slot);
                _inputBuffers[slot].Dispose();
                _inputBuffers.RemoveAtSwapBack(slot);
                --slot;
            }
        }

        private void AcceptNewConnections()
        {
            NetworkConnection connection;
            while ((connection = _driver.Accept()) != default)
            {
                _connections.Add(connection);
                _inputBuffers.Add(InputBuffer.Create());
                Debug.Log("Accepted a connection");
            }
        }
        
        protected override void OnDestroy()
        {
            _driver.Dispose();
            _connections.Dispose();
            
            foreach (InputBuffer inputBuffer in _inputBuffers)
            {
                inputBuffer.Dispose();
            }
            _inputBuffers.Dispose();
        }
    }
}