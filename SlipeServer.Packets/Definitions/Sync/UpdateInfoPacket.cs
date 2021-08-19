using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element
{
    public class UpdateInfoPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_UPDATE_INFO;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public string Type { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;

        public UpdateInfoPacket()
        {

        }

        public UpdateInfoPacket(string type, string data)
        {
            this.Type = type;
            this.Data = data;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write(this.Type);
            builder.Write(this.Data);

            return builder.Build();
        }
    }
}
