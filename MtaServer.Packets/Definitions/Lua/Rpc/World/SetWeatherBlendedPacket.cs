using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.Rpc.World
{
    public class SetWeatherBlendedPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;

        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;
        public byte Weather { get; set; }
        public byte Hour { get; set; }
        public SetWeatherBlendedPacket(byte weather,byte hour)
        {
            Weather = weather;
            Hour = hour;
        }
        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            PacketBuilder builder = new PacketBuilder();
            builder.Write((byte)ElementRPCFunction.SET_WEATHER_BLENDED);
            builder.Write(this.Weather);
            builder.Write(this.Hour);
            return builder.Build();
        }
    }
}
