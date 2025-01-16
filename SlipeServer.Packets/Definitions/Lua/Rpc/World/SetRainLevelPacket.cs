using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public sealed class SetRainLevelPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public float RainLevel { get; set; }

    public SetRainLevelPacket(float rainLevel)
    {
        this.RainLevel = rainLevel;
    }

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_RAIN_LEVEL);
        builder.Write(this.RainLevel);

        return builder.Build();
    }
}
