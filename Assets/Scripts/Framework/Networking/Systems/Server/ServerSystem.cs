using Framework.Networking.Components;
using Framework.Networking.Messages;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Assertions;

namespace Framework.Networking.Systems.Server
{
    struct ServerUpdateConnectionsJob : IJob
    {
        public NetworkDriver Driver;
        public NativeList<NetworkConnection> Connections;
        //public NativeList<InputBuffer> InputBuffers;

        public void Execute()
        {
            CleanupConnections();
            AcceptNewConnections();
        }
        
        private void CleanupConnections()
        {
            for (int slot = 0; slot < Connections.Length; slot++)
            {
                if (Connections[slot].IsCreated) continue;
                Connections.RemoveAtSwapBack(slot);
                //InputBuffers[slot].Dispose();
                //InputBuffers.RemoveAtSwapBack(slot);
                --slot;
            }
        }

        private void AcceptNewConnections()
        {
            NetworkConnection connection;
            while ((connection = Driver.Accept()) != default)
            {
                Connections.Add(connection);
                //InputBuffers.Add(InputBuffer.Create());
                Debug.Log("Accepted a connection");
            }
        }
    }

    struct ServerUpdateJob : IJobParallelForDefer
    {
        public NetworkDriver.Concurrent Driver;
        public NativeArray<NetworkConnection> Connections;
        //public NativeList<InputBuffer> InputBuffers;

        public void Execute(int slot)
        {
            Assert.IsTrue(Connections[slot].IsCreated);

            NetworkEvent.Type cmd;
            while ((cmd = Driver.PopEventForConnection(Connections[slot], out DataStreamReader stream)) != NetworkEvent.Type.Empty)
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
                                
                                Driver.BeginSend(NetworkPipeline.Null, Connections[slot], out DataStreamWriter writer);
                                response.Serialize(ref writer);
                                Driver.EndSend(writer);
                                break;
                            
                            case MessageType.PlayerInput:
                                //InputBuffers[slot].Write(PlayerInputMessage.Deserialize(stream));
                                // Fire an event here with the player input message as the payload
                                // We still need a way of getting the correct entity to update
                                // map the entity with slot when it is possessed and store it in a native map here
                                // set the entity with the input data on an event component
                                // in the event handler...
                                // can we do the event handler here?
                                // we just need a command buffer we can use in this job
                                // reduces boilerplate!
                                break;
                            
                            case MessageType.SpawnEntity:
                                // TODO we need to map a player slot to a specific entity so that we can apply inputs
                                // Spawn entity from prefab
                                // generate a network id
                                // add Replicated component
                                // get the entity id from the message (we send it back to the owner with the network id so they can map it)
                                // send network id and entity id back to owner
                                // broadcast entity spawn to other connections
                                break;
                            
                            case MessageType.PossessEntity:
                                // need some kind of check to see if this is allowed
                                // add PlayerSlot component
                                // add InputBuffer component
                                // store a ref to the buffer in an array here so we can update it
                                break;
                        }
                        
                        break;
                    }
                    case NetworkEvent.Type.Disconnect:
                        Debug.Log("Client disconnected from server");
                        Connections[slot] = default;
                        break;
                }
            }
        }
    }
    
    class ServerSystem : JobComponentSystem {
        private NetworkDriver _driver;
        private NativeList<NetworkConnection> _connections;
        //private NativeList<InputBuffer> _inputBuffers;
        private NetworkIdSystem _networkIdSystem;

        protected override void OnCreate() {
            RequireSingletonForUpdate<Tags.Server>();
            _networkIdSystem = World.GetOrCreateSystem<NetworkIdSystem>();
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
            //_inputBuffers = new NativeList<InputBuffer>(16, Allocator.Persistent);
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
            
        // TODO have a job to send out snapshots for replicated entities
        // TODO have a job to send out snapshots for predicted entities

        // simulate possessed entities
        // TODO use shared component data for input buffers
        // send out snapshots for replicated entities
            
        // send out snapshots for predicted entities
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var connectionJob = new ServerUpdateConnectionsJob
            {
                Driver = _driver,
                Connections = _connections,
                //InputBuffers = _inputBuffers
            };

            var serverUpdateJob = new ServerUpdateJob
            {
                Driver = _driver.ToConcurrent(),
                Connections = _connections.AsDeferredJobArray(),
                //InputBuffers = _inputBuffers
            };

            inputDeps = _driver.ScheduleUpdate(inputDeps);
            inputDeps = connectionJob.Schedule(inputDeps);
            inputDeps = serverUpdateJob.Schedule(_connections, 1, inputDeps);
            
            return inputDeps;
        }

        protected override void OnDestroy()
        {
            _driver.Dispose();
            _connections.Dispose();
            
            // foreach (InputBuffer inputBuffer in _inputBuffers)
            // {
            //     inputBuffer.Dispose();
            // }
            // _inputBuffers.Dispose();
        }
    }
}