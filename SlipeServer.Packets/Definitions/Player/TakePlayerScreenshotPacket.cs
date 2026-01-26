using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Player;

public sealed class TakePlayerScreenshotPacket(ushort sizeX, ushort sizeY, string tag, byte quality, uint maxBandwith, ushort maxPacketSize, ushort resourceId) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_LUA;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ushort SizeX { get; set; } = sizeX;
    public ushort SizeY { get; set; } = sizeY;
    public string Tag { get; set; } = tag;
    public byte Quality { get; set; } = quality;
    public uint MaxBandwith { get; set; } = maxBandwith;
    public ushort MaxPacketSize { get; set; } = maxPacketSize;
    public ushort ResourceId { get; set; } = resourceId;
    public string ResourceName { get; set; } = "unknown";

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write((byte)ElementRpcFunction.TAKE_PLAYER_SCREEN_SHOT);

        builder.Write(this.SizeX);
        builder.Write(this.SizeY);
        builder.Write(this.Tag);
        builder.Write(this.Quality);
        builder.Write(this.MaxBandwith);
        builder.Write(this.MaxPacketSize);
        builder.Write(this.ResourceId);
        builder.Write(this.ResourceName);
        builder.Write((uint)0);

        return builder.Build();
    }
}
