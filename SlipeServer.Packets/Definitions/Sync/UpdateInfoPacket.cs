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

        public string Type { get; set; }
        public string Data { get; set; }

        public UpdateInfoPacket()
        {

        }

        public UpdateInfoPacket(string type, string data)
        {
            Type = type;
            Data = data;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write(Type);
            builder.Write(Data);

            return builder.Build();
        }
    }
}
