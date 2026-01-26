using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System.Collections.Generic;

namespace SlipeServer.Packets.Definitions.Resources;

public sealed class ResourceClientScriptsPacket(
    ushort netId,
    Dictionary<string, byte[]> files
    ) : Packet
{

    public override PacketId PacketId => PacketId.PACKET_ID_RESOURCE_CLIENT_SCRIPTS;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public ushort NetId { get; } = netId;
    public Dictionary<string, byte[]> Files { get; } = files;

    public override void Read(byte[] bytes)
    {
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.Write(this.NetId);
        builder.Write((ushort)this.Files.Count);

        foreach (var kvPair in this.Files)
        {
            builder.Write(kvPair.Key);
            builder.Write((uint)kvPair.Value.Length);
            builder.Write(kvPair.Value);
        }


        return builder.Build();
    }
}
