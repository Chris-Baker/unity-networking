using Framework.Networking.Components;
using Framework.Networking.Messages;
using Game.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;

namespace Framework.Networking.Systems.Client
{
    struct ClientUpdateJob : IJob
    {
        public NetworkDriver Driver;
        public NativeArray<NetworkConnection> Connection;
        public EntityCommandBuffer.ParallelWriter CommandBuffer;
        public Prefabs Prefabs;

        public void Execute()
        {
            NetworkConnection connection = Connection[0];
            
            if (!connection.IsCreated)
            {
                Debug.Log("Something went wrong during connect");
                return;
            }

            NetworkEvent.Type cmd;
            while ((cmd = connection.PopEvent(Driver, out DataStreamReader stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    Debug.Log("We are now connected to the server");

                    // TODO send initial joining data
                    // player info etc

                    ConnectionInfoMessage request = new ConnectionInfoMessage()
                    {
                        Name = "Player 1"
                    };
                    
                    Driver.BeginSend(connection, out DataStreamWriter writer);
                    request.Serialize(ref writer);
                    Driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    byte messageType = stream.ReadByte();

                    switch (messageType)
                    {
                        case MessageType.ConnectionResult:
                            ConnectionResultMessage response = ConnectionResultMessage.Deserialize(stream);
                            Debug.Log("Received message from server: " + response.Message);
                            
                            // spawn a player entity
                            // TODO fire off a player spawn event here
                            Broken code

                            break;
                        
                        case MessageType.ReplicatedSnapshot:
                            // contains snapshot data for a network entity
                            break;
                        
                        case MessageType.PredictedSnapshot:
                            // contains snapshot data for a network entity
                            // and the prediction id
                            break;
                        
                        case MessageType.NetworkId:
                            // contains a network id
                            // and the corresponding entity id so we can add a network id to an already spawned entity
                            break;
                        
                        case MessageType.SpawnEntity:
                            // spawn a replicated entity
                            break;
                        
                        case MessageType.PossessEntity:
                            // map our inputs to the specified entity
                            // add a player input component to the entity
                            // add a predicted entity component to the entity
                            break;
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client got disconnected from server");
                    connection = default(NetworkConnection);
                }
            }
        }
    }
    
    class ClientSystem : JobComponentSystem {
        private NetworkDriver _driver;
        private NativeArray<NetworkConnection> _connection;
        
        private BeginInitializationEntityCommandBufferSystem _entityCommandBufferSystem;
        
        protected override void OnCreate() {
            RequireSingletonForUpdate<Tags.Client>();
            _entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        protected override void OnStartRunning()
        {
            _driver = NetworkDriver.Create();
            _connection = new NativeArray<NetworkConnection>(1, Allocator.Persistent);

            NetworkEndPoint endpoint = NetworkEndPoint.LoopbackIpv4;
            endpoint.Port = 9000;
            _connection[0] = _driver.Connect(endpoint);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityCommandBuffer.ParallelWriter commandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            ClientUpdateJob clientUpdateJob = new ClientUpdateJob
            {
                Driver = _driver,
                Connection = _connection,
                CommandBuffer = commandBuffer
            };
            
            inputDeps = _driver.ScheduleUpdate(inputDeps);
            inputDeps = clientUpdateJob.Schedule(inputDeps);
            
            _entityCommandBufferSystem.AddJobHandleForProducer(inputDeps);
            
            return inputDeps;
        }
        
        // protected override void OnUpdate()
        // {
        //     _driver.ScheduleUpdate().Complete();
        //
        //     if (!_connection.IsCreated)
        //     {
        //         Debug.Log("Something went wrong, lost connection to server!");
        //         return;
        //     }
        //     
        //     Receive();
        //     //Send();
        // }

        // private void Send()
        // {
        //     float deltaTime = Time.DeltaTime;
        //     
        //     // Get the player input singleton
        //     PlayerInput input = GetSingleton<PlayerInput>();
        //
        //     // Create a player input message
        //     PlayerInputMessage message = new PlayerInputMessage
        //     {
        //         Up = input.Up,
        //         Down = input.Down,
        //         Left = input.Left,
        //         Right = input.Right,
        //         Shoot = input.Shoot,
        //         Use = input.Use,
        //         Sprint = input.Sprint,
        //         Jump = input.Jump,
        //         Yaw = input.Yaw,
        //         Pitch = input.Pitch,
        //         PredictionId = input.PredictionId,
        //         DeltaTime = deltaTime
        //     };
        //
        //     // Send the message to server
        //     _driver.BeginSend(_connection, out DataStreamWriter writer);
        //     message.Serialize(ref writer);
        //     _driver.EndSend(writer);
        // }

        protected override void OnDestroy()
        {
            _driver.Dispose();
            _connection.Dispose();
        }
    }
}