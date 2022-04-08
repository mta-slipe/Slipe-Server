using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipeServer.Packets.Definitions.Resources;

public class ResourceStopPacket : Packet
{
    public override PacketId PacketId => PacketId.PACKET_ID_RESOURCE_STOP;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ushort NetId { get; }

    public ResourceStopPacket(
        ushort netId
    )
    {
        this.NetId = netId;
    }

    public override void Read(byte[] bytes)
    {
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.NetId);

        return builder.Build();
    }
}
