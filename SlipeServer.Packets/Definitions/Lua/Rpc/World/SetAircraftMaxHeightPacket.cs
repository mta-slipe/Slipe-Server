using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetAircraftMaxHeightPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

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
