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

    public readonly struct ServerResourceFile
    {
        public required string Name { get; init; }
        public required ResourceFileType FileType { get; init; }
        public required byte[] Content { get; init; }
    }
}
