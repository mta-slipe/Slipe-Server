using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetSunColorPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;

        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public Color CoreSun { get; set; }
        public Color CoronaSun { get; set; }

        public SetSunColorPacket(Color coreColor, Color coronaColor)
        {
            CoreSun = coreColor;
            CoronaSun = coronaColor;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.SET_SUN_COLOR);
            builder.Write(this.CoreSun);
            builder.Write(this.CoronaSun);

            return builder.Build();
        }
    }
}
