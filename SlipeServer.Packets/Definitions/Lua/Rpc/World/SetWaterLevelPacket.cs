using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public sealed class SetWaterLevelPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public float Level { get; set; }
    public bool IncludeNonSeaLevel { get; set; }
    public bool IncludeWorldSeaLevel { get; set; }
    public bool IncludeOutsideWorldLevel { get; set; }

    public SetWaterLevelPacket(float level, bool includeNonSeaLevel, bool includeWorldSeaLevel, bool includeOutsideWorldLevel)
    {
        this.Level = level;
        this.IncludeNonSeaLevel = includeNonSeaLevel;
        this.IncludeWorldSeaLevel = includeWorldSeaLevel;
        this.IncludeOutsideWorldLevel = includeOutsideWorldLevel;
    }
    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_WORLD_WATER_LEVEL);
        builder.Write(this.Level);
        builder.Write(this.IncludeNonSeaLevel);
        builder.Write(this.IncludeWorldSeaLevel);
        builder.Write(this.IncludeOutsideWorldLevel);

        return builder.Build();
    }
}
