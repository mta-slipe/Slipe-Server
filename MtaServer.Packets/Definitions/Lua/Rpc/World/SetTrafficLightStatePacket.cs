using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetTrafficLightStatePacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;

        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;
        public byte State { get; set; }
        public bool Forced { get; set; }

        public SetTrafficLightStatePacket(byte state, bool forced)
        {
            State = state;
            Forced = forced;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.SET_TRAFFIC_LIGHT_STATE);
            builder.WriteBytesCapped(BitConverter.GetBytes(this.State),4);
            builder.Write(this.Forced);

            return builder.Build();
        }
    }
}
