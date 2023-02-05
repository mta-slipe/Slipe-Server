using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Collections.Generic;

namespace SlipeServer.Packets.Definitions.Player;

public class PlayerStatsPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_STATS;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public PlayerNetworkStatusType Type { get; set; }

    public ElementId ElementId { get; set; }
    public Dictionary<ushort, float> Stats { get; set; }

    public PlayerStatsPacket()
    {
        this.Stats = new();
    }

    public override void Read(byte[] bytes)
    {
        throw new NotSupportedException();
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.ElementId);
        builder.WriteCompressed((ushort)Stats.Count);
        foreach (var kvPair in this.Stats)
        {
            builder.Write(kvPair.Key);
            builder.Write(kvPair.Value);
        }

        return builder.Build();
    }
}
