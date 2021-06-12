using System;
using Unity.Collections;
using Unity.Networking.Transport;

namespace Framework.Networking.Messages
{
    public static class MessageType
    {
        public const byte ConnectionInfo        = 1;
        public const byte ConnectionResult      = 2;
        public const byte PlayerInput           = 3;
        public const byte ReplicatedSnapshot    = 4;
        public const byte PredictedSnapshot     = 5;
        public const byte NetworkId             = 6;
        public const byte SpawnEntity           = 7;
        public const byte PossessEntity         = 8;
    }
    
    public struct ConnectionInfoMessage
    {
        public FixedString128 Name;
        
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteByte(MessageType.ConnectionInfo);
            writer.WriteFixedString128(Name);
        }

        public static ConnectionInfoMessage Deserialize(DataStreamReader stream)
        {
            return new ConnectionInfoMessage
            {
                Name = stream.ReadFixedString128()
            };
        }
    }
    
    public struct ConnectionResultMessage
    {
        public FixedString128 Message;
        
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteByte(MessageType.ConnectionResult);
            writer.WriteFixedString128(Message);
        }

        public static ConnectionResultMessage Deserialize(DataStreamReader stream)
        {
            return new ConnectionResultMessage
            {
                Message = stream.ReadFixedString128()
            };
        }
    }
    
    public struct PlayerInputMessage
    {
        public bool Up, Down, Left, Right, Shoot, Use, Sprint, Jump;
        public float Yaw, Pitch;
        public uint PredictionId;
        public float DeltaTime;
        
        public void Serialize(ref DataStreamWriter writer)
        {
            int inputs = 0;
            inputs |= Up ? 1 << 0 : 0;
            inputs |= Down ? 1 << 1 : 0;
            inputs |= Left ? 1 << 2 : 0;
            inputs |= Right ? 1 << 3 : 0;
            inputs |= Shoot ? 1 << 4 : 0;
            inputs |= Use ? 1 << 5 : 0;
            inputs |= Sprint ? 1 << 6 : 0;
            inputs |= Jump ? 1 << 7 : 0;
            
            writer.WriteByte(MessageType.PlayerInput);
            writer.WriteByte(Convert.ToByte(inputs));
            writer.WriteFloat(Yaw);
            writer.WriteFloat(Pitch);
            writer.WriteUInt(PredictionId);
            writer.WriteFloat(DeltaTime);
        }

        public static PlayerInputMessage Deserialize(DataStreamReader stream)
        {
            byte inputs = stream.ReadByte();
            float yaw = stream.ReadFloat();
            float pitch = stream.ReadFloat();
            uint predictionId = stream.ReadUInt();
            float deltaTime = stream.ReadFloat();
            
            return new PlayerInputMessage
            {
                Up = (inputs & (1 << 0)) != 0,
                Down = (inputs & (1 << 1)) != 0,
                Left = (inputs & (1 << 2)) != 0,
                Right = (inputs & (1 << 3)) != 0,
                Shoot = (inputs & (1 << 4)) != 0,
                Use = (inputs & (1 << 5)) != 0,
                Sprint = (inputs & (1 << 6)) != 0,
                Jump = (inputs & (1 << 7)) != 0,
                Yaw = yaw,
                Pitch = pitch,
                PredictionId = predictionId,
                DeltaTime = deltaTime
            };
        }
    }

    public struct ReplicatedSnapshot
    {
        public Snapshot Snapshot;

        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteByte(MessageType.ReplicatedSnapshot);
            Snapshot.Serialize(ref writer);
        }

        public static ReplicatedSnapshot Deserialize(DataStreamReader stream)
        {
            return new ReplicatedSnapshot
            {
                Snapshot = Snapshot.Deserialize(stream)
            };
        }
    }
    
    public struct PredictedSnapshot
    {
        public Snapshot Snapshot;
        public uint PredictionId;
        
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteByte(MessageType.PredictedSnapshot);
            Snapshot.Serialize(ref writer);
            writer.WriteUInt(PredictionId);
        }

        public static PredictedSnapshot Deserialize(DataStreamReader stream)
        {
            return new PredictedSnapshot
            {
                Snapshot = Snapshot.Deserialize(stream),
                PredictionId = stream.ReadUInt()
            };
        }
    }
    
    public struct Snapshot
    {
        public float X, Y, Z;
        public float Yaw, Pitch, Roll;

        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteFloat(X);
            writer.WriteFloat(Y);
            writer.WriteFloat(Z);
            writer.WriteFloat(Yaw);
            writer.WriteFloat(Pitch);
            writer.WriteFloat(Roll);
        }

        public static Snapshot Deserialize(DataStreamReader stream)
        {
            return new Snapshot
            {
                X = stream.ReadFloat(),
                Y = stream.ReadFloat(),
                Z = stream.ReadFloat(),
                Yaw = stream.ReadFloat(),
                Pitch = stream.ReadFloat(),
                Roll = stream.ReadFloat()
            };
        }
    }
}