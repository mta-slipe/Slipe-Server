using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Packets.Definitions.Resources;

public class ResourceStartPacket : Packet
{

    public override PacketId PacketId => PacketId.PACKET_ID_RESOURCE_START;
    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
    public override PacketPriority Priority => PacketPriority.High;

    public string Name { get; set; }
    public ushort NetId { get; set; }
    public ElementId ResourceDynamicElementId { get; set; }
    public ushort UncachedScriptCount { get; }
    public string MinServerVersion { get; }
    public string MinClientVersion { get; }
    public bool IsOopEnabled { get; }
    public int DownloadPriorityGroup { get; }
    public IEnumerable<ResourceFile> Files { get; }
    public IEnumerable<string> ExportedFunctions { get; }
    public ElementId ResourceElementId { get; }

    public ResourceStartPacket()
    {

    }

    public ResourceStartPacket(
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
    )
    {
        this.Name = name;
        this.NetId = netId;
        this.ResourceElementId = resourceElementId;
        this.ResourceDynamicElementId = resourceDynamicElementId;
        this.UncachedScriptCount = uncachedScriptCount;
        this.MinServerVersion = minServerVersion ?? "";
        this.MinClientVersion = minClientVersion ?? "";
        this.IsOopEnabled = isOopEnabled;
        this.DownloadPriorityGroup = downloadPriorityGroup;
        this.Files = files;
        this.ExportedFunctions = exportedFunctions;
    }

    public override void Read(byte[] bytes)
    {
        var reader = new PacketReader(bytes);
        var len = reader.GetByte();
        this.Name = Encoding.UTF8.GetString(reader.GetBytes(len));
        this.NetId = reader.GetUInt16();
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
