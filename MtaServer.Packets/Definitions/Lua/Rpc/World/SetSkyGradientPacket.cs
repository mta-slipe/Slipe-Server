using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetSkyGradientPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;

        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public Color Top { get; set; }
        public Color Bottom { get; set; }

        public SetSkyGradientPacket(Color? topColor = null, Color? bottomColor = null)
        {
            Top = topColor ?? Color.FromArgb(0, 0, 0);
            Bottom = bottomColor ?? Color.FromArgb(0, 0, 0);
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.SET_SKY_GRADIENT);
            builder.Write(this.Top);
            builder.Write(this.Bottom);

            return builder.Build();
        }
    }
}
