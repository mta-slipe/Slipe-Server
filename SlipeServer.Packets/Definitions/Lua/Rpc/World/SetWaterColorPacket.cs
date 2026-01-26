using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Drawing;

namespace SlipeServer.Packets.Definitions.Lua.Rpc.World;

public sealed class SetWaterColorPacket(Color color) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public Color Color { get; set; } = color;

    public override void Read(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public override byte[] Write()
    {
        PacketBuilder builder = new PacketBuilder();
        builder.Write((byte)ElementRPCFunction.SET_WATER_COLOR);
        builder.Write(this.Color, true, false);

        return builder.Build();
    }
}
