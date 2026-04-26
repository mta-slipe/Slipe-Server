using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Resources;

namespace SlipeServer.DropInReplacement.MixedResources;

public class MixedResource(IMtaServer server, IRootElement root, string name, string? path = null) : Resource(server, root, name, path)
{
    public List<ServerResourceFile> ServerFiles { get; init; } = [];
    public List<string> ServerExports { get; init; } = [];
    public Dictionary<string, string> Settings { get; init; } = [];
    public List<IncludedResource> IncludedResources { get; init; } = [];
    public List<MapDefinition> Maps { get; init; } = [];
    public List<HtmlDefinition> HtmlFiles { get; init; } = [];
    public MinMtaVersion? MinimumMtaVersion { get; init; }
    public List<AclRight> AclRequestRights { get; init; } = [];
    public bool? SyncMapElementData { get; init; }

    public readonly struct IncludedResource
    {
        public required string ResourceName { get; init; }
        public string? MinimumVersion { get; init; }
        public string? MaximumVersion { get; init; }
    }

    public readonly struct MapDefinition
    {
        public required string Source { get; init; }
        public int? Dimension { get; init; }
    }

    public readonly struct HtmlDefinition
    {
        public required string Source { get; init; }
        public bool IsDefault { get; init; }
        public bool IsRaw { get; init; }
    }

    public readonly struct MinMtaVersion
    {
        public string? Client { get; init; }
        public string? Server { get; init; }
        public string? Both { get; init; }
    }

    public readonly struct AclRight
    {
        public required string Name { get; init; }
        public bool Access { get; init; }
    }

    public readonly struct ServerResourceFile
    {
        public required string Name { get; init; }
        public required ResourceFileType FileType { get; init; }
        public required byte[] Content { get; init; }
    }
}
