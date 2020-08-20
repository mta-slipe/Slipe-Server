using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using MtaServer.Packets.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtaServer.Packets.Definitions.Resources
{
    public class ResourceStartPacket : Packet
    {

        public override PacketId PacketId => PacketId.PACKET_ID_RESOURCE_START;
        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public string Name { get; set; }
        public ushort NetId { get; }
        public uint ResourceDynamicElementId { get; set; }
        public ushort UncachedScriptCount { get; }
        public string MinServerVersion { get; }
        public string MinClientVersion { get; }
        public bool IsOopEnabled { get; }
        public int DownloadPriorityGroup { get; }
        public IEnumerable<ResourceFile> Files { get; }
        public IEnumerable<string> ExportedFunctions { get; }
        public uint ResourceElementId { get; }

        public ResourceStartPacket(
            string name,
            ushort netId,
            uint resourceElementId,
            uint resourceDynamicElementId,
            ushort uncachedScriptCount,
            string? minServerVersion,
            string? minClientVersion,
            bool isOopEnabled,
            int downloadPriorityGroup,
            IEnumerable<ResourceFile> files,
            IEnumerable<string> exportedFunctions
        )
        {
            Name = name;
            NetId = netId;
            ResourceElementId = resourceElementId;
            ResourceDynamicElementId = resourceDynamicElementId;
            UncachedScriptCount = uncachedScriptCount;
            MinServerVersion = minServerVersion ?? "";
            MinClientVersion = minClientVersion ?? "";
            IsOopEnabled = isOopEnabled;
            DownloadPriorityGroup = downloadPriorityGroup;
            Files = files;
            ExportedFunctions = exportedFunctions;
        }

        public override void Read(byte[] bytes)
        {
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteStringWithByteAsLength(Name);
            builder.Write(NetId);
            builder.WriteElementId(ResourceElementId);
            builder.WriteElementId(ResourceDynamicElementId);

            builder.Write(UncachedScriptCount);
            builder.Write(MinServerVersion);
            builder.Write(MinClientVersion);
            builder.Write(IsOopEnabled);
            builder.Write(DownloadPriorityGroup);

            foreach (var file in Files)
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

            foreach (var export in ExportedFunctions)
            {
                builder.Write((byte)'E');
                builder.WriteStringWithByteAsLength(export);
            }

            return builder.Build();
        }
    }
}
