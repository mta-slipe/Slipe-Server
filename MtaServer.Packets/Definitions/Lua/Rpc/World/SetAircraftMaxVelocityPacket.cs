using MtaServer.Packets.Builder;
using MtaServer.Packets.Definitions.Lua.ElementRpc;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetAircraftMaxVelocityPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;

        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public float MaxVelocity { get; set; }

        public SetAircraftMaxVelocityPacket(float maxVelocity)
        {
            this.MaxVelocity = maxVelocity;
        }

        public override void Read(byte[] bytes)
        {
            
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.SET_AIRCRAFT_MAXVELOCITY);
            builder.Write(this.MaxVelocity);

            return builder.Build();
        }
    }
}
