using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Structs;
using System.Collections.Generic;

namespace SlipeServer.Packets.Definitions.Resources;

public sealed class ResourceStartPacket(
    string name,
    ushort netId,
    ElementId resourceElementId,
    ElementId resourceDynamicElementId,
    ushort uncachedScriptCount,
    string? minServerVersion,
    string? minClientVersion,
    bool isOopEnabled,
    int downloadPriorityGroup,
    IEnumerable<ResourceFile> files,
    IEnumerable<string> exportedFunctions
    ) : Packet
{

    public override PacketId PacketId => PacketId.PACKET_ID_RESOURCE_START;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public string Name { get; set; } = name;
    public ushort NetId { get; } = netId;
    public ElementId ResourceDynamicElementId { get; set; } = resourceDynamicElementId;
    public ushort UncachedScriptCount { get; } = uncachedScriptCount;
    public string MinServerVersion { get; } = minServerVersion ?? "";
    public string MinClientVersion { get; } = minClientVersion ?? "";
    public bool IsOopEnabled { get; } = isOopEnabled;
    public int DownloadPriorityGroup { get; } = downloadPriorityGroup;
    public IEnumerable<ResourceFile> Files { get; } = files;
    public IEnumerable<string> ExportedFunctions { get; } = exportedFunctions;
    public ElementId ResourceElementId { get; } = resourceElementId;

    public override void Read(byte[] bytes)
    {
    }

    public override byte[] Write()
    {
        var builder = new PacketBuilder();

        builder.WriteStringWithByteAsLength(this.Name);
        builder.Write(this.NetId);
        builder.Write(this.ResourceElementId);
        builder.Write(this.ResourceDynamicElementId);

        builder.Write(this.UncachedScriptCount);
        builder.Write(this.MinServerVersion);
        builder.Write(this.MinClientVersion);
        builder.Write(this.IsOopEnabled);
        builder.Write(this.DownloadPriorityGroup);

        foreach (var file in this.Files)
        {
            builder.Write((byte)'F');

            builder.WriteStringWithByteAsLength(file.Name.Replace("\\", "/"));
            builder.Write(file.FileType);
            builder.Write((uint)file.CheckSum);
            builder.Write(file.Md5);
            builder.Write(file.AproximateSize);

            if (file.IsAutoDownload.HasValue)
            {
                builder.Write(file.IsAutoDownload.Value);
            }
        }

        foreach (var export in this.ExportedFunctions)
        {
            builder.Write((byte)'E');
            builder.WriteStringWithByteAsLength(export);
        }

        return builder.Build();
    }
}
