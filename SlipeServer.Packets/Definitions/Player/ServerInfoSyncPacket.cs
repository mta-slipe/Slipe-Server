using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Player;

public sealed class ServerInfoSyncPacket(uint maxPlayers) : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_SERVER_INFO_SYNC;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public byte ActualInfo { get; set; } = 1;
    public uint MaxPlayers { get; set; } = maxPlayers;

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();
        builder.Write(this.ActualInfo);
        builder.Write(this.MaxPlayers);
        return builder.Build();
    }
}
