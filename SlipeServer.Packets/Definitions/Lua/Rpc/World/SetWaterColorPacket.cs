using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetWaterColorPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public Color Color { get; set; }
        public bool Forced { get; set; }

        public SetWaterColorPacket(Color color)
        {
            this.Color = color;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.SET_WATER_COLOR);
            builder.Write(this.Color, true);

            return builder.Build();
        }
    }
}
