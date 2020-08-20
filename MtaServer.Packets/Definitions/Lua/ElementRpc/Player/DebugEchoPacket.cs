using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.ElementRpc.Player
{
    public class DebugEchoPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_DEBUG_ECHO;
        public override PacketFlags Flags => PacketFlags.PACKET_LOW_PRIORITY;

        public string Message { get; set; }
        public byte Level { get; set; }
        public Color Color { get; set; }

        public DebugEchoPacket(string message, byte level, Color color)
        {
            Message = message;
            Level = level;
            Color = color;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write(Level);
            if(Level == 0)
            {
                builder.Write(Color);
            }
            builder.WriteStringWithoutLength(Message);

            return builder.Build();
        }
    }

}
