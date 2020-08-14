using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetTimePacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;

        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public int Hour { get; set; }
        public int Minute { get; set; }

        public SetTimePacket(int hour,int minute)
        {
            Hour = hour;
            Minute = minute;
        }
        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.SET_TIME);
            builder.Write(this.Hour);
            builder.Write(this.Minute);

            return builder.Build();
        }
    }
}
