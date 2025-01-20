using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;

namespace SlipeServer.Packets.Definitions.Player;

public sealed class PlayerNetworkStatusPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_NETWORK_STATUS;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public PlayerNetworkStatusType Type { get; set; }
    public uint Ticks { get; set; }

    public PlayerNetworkStatusPacket()
    {
    }

    public override void Read(byte[] bytes)
    {
        PacketReader reader = new PacketReader(bytes);
        this.Type = (PlayerNetworkStatusType)reader.GetByte();
        this.Ticks = reader.GetUint32();
    }

    public override byte[] Write()
    {
        throw new NotSupportedException();
    }
}
