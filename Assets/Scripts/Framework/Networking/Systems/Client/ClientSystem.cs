using Framework.Networking.Components;
using Framework.Networking.Messages;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace Framework.Networking.Systems.Client
{
    class ClientSystem : ComponentSystem {
        private NetworkDriver _driver;

        public NetworkDriver Driver => _driver;

        private NetworkConnection _connection;
        
        public NetworkConnection Connection => _connection;

        protected override void OnCreate() {
            RequireSingletonForUpdate<Tags.Client>();
        }

        protected override void OnStartRunning()
        {
            _driver = NetworkDriver.Create();
            _connection = default;

            NetworkEndPoint endpoint = NetworkEndPoint.LoopbackIpv4;
            endpoint.Port = 9000;
            _connection = _driver.Connect(endpoint);
        }

        protected override void OnUpdate()
        {
            _driver.ScheduleUpdate().Complete();

            if (!_connection.IsCreated)
            {
                Debug.Log("Something went wrong, lost connection to server!");
                return;
            }
            
            Receive();
            Send();
        }

        private void Send()
        {
            float deltaTime = Time.DeltaTime;
            
            // Get the player input singleton
            PlayerInput input = GetSingleton<PlayerInput>();

            // Create a player input message
            PlayerInputMessage message = new PlayerInputMessage
            {
                Up = input.Up,
                Down = input.Down,
                Left = input.Left,
                Right = input.Right,
                Shoot = input.Shoot,
                Use = input.Use,
                Sprint = input.Sprint,
                Jump = input.Jump,
                Yaw = input.Yaw,
                Pitch = input.Pitch,
                PredictionId = input.PredictionId,
                DeltaTime = deltaTime
            };

            // Send the message to server
            _driver.BeginSend(_connection, out DataStreamWriter writer);
            message.Serialize(ref writer);
            _driver.EndSend(writer);
        }
        
        private void Receive()
        {
            NetworkEvent.Type cmd;

            while ((cmd = _connection.PopEvent(_driver, out DataStreamReader stream)) != NetworkEvent.Type.Empty)
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
                    
                    _driver.BeginSend(_connection, out DataStreamWriter writer);
                    request.Serialize(ref writer);
                    _driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    byte messageType = stream.ReadByte();

                    switch (messageType)
                    {
                        case MessageType.ConnectionResult:
                            ConnectionResultMessage response = ConnectionResultMessage.Deserialize(stream);
                            Debug.Log("Received message from server: " + response.Message);
                            break;
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client got disconnected from server");
                    _connection = default(NetworkConnection);
                }
            }
        }
        
        protected override void OnDestroy()
        {
            _driver.Dispose();
        }
    }
}