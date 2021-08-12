using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element
{
    public class PlayerDisconnectPacket : Packet
    { 
        public override PacketId PacketId => PacketId.PACKET_ID_SERVER_DISCONNECTED;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;


        public PlayerDisconnectType Type { get; set; }
        public long Duration { get; set; }
        public string Reason { get; set; }

        public PlayerDisconnectPacket(string reason)
        {
            this.Type = PlayerDisconnectType.CUSTOM;
            this.Reason = reason;
            this.Duration = 0;
        }

        public PlayerDisconnectPacket(PlayerDisconnectType type, string reason, int duration = 0)
        {
            this.Type = type;
            this.Reason = reason;
            this.Duration = duration;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteCapped((int)Type, 5);

            if(Type == PlayerDisconnectType.BAN || Type == PlayerDisconnectType.BANNED_SERIAL || Type == PlayerDisconnectType.BANNED_IP)
            {
                builder.Write(Duration.ToString());
            }

            if(!string.IsNullOrEmpty(Reason))
            {
                builder.Write(Reason);
            }
            return builder.Build();
        }
    }
}
