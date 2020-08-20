using MtaServer.Packets.Builder;
using MtaServer.Packets.Definitions.Lua.ElementRpc;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetAircraftMaxHeightPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;

        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public float MaxHeight { get; set; }

        public SetAircraftMaxHeightPacket(float maxHeight)
        {
            this.MaxHeight = maxHeight;
        }

        public override void Read(byte[] bytes)
        {
           
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.SET_AIRCRAFT_MAXHEIGHT);
            builder.Write(this.MaxHeight);

            return builder.Build();
        }
    }
}
