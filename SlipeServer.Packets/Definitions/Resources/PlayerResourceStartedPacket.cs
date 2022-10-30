using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;

namespace SlipeServer.Packets.Definitions.Resources;

public class PlayerResourceStartedPacket : Packet
{

    public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_RESOURCE_START;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ushort NetId { get; set; }

    public PlayerResourceStartedPacket()
    {
    }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);

        this.NetId = reader.GetUint16();
    }

    public override byte[] Write()
    {
        throw new NotSupportedException();
    }
}
