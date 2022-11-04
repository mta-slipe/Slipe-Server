using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public class SetWeatherBlendedPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public byte Weather { get; set; }
    public byte Hour { get; set; }
    public SetWeatherBlendedPacket(byte weather, byte hour)
    {
        this.Weather = weather;
        this.Hour = hour;
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
