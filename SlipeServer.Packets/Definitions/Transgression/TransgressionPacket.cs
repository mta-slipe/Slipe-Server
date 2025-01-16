using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;

namespace SlipeServer.Packets.Definitions.Transgression;

public sealed class TransgressionPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_TRANSGRESSION;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public uint Level { get; set; }
    public string Message { get; set; } = string.Empty;

    public TransgressionPacket()
    {
    }

    public override byte[] Write()
    {
        throw new NotSupportedException();
    }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);

        this.Level = reader.GetUint32();
        this.Message = reader.GetString();
    }
}
