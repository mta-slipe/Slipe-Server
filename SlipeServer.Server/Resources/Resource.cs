using SlipeServer.Packets.Definitions.Resources;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Resources.Providers;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.Resources;

public class Resource
{
    private readonly MtaServer server;
    private readonly IResourceProvider resourceProvider;

    public DummyElement Root { get; }
    public DummyElement DynamicRoot { get; }
    public ushort NetId { get; set; }
    public int PriorityGroup { get; set; }
    public List<string> Exports { get; }
    public Dictionary<string, byte[]> NoClientScripts { get; set; }
    public string Name { get; }
    public string Path { get; }

    public Resource(MtaServer server, RootElement root, IResourceProvider resourceProvider, string name, string? path = null)
    {
        this.server = server;
        this.resourceProvider = resourceProvider;
        this.Name = name;
        this.Path = path ?? $"./{name}";

        this.NoClientScripts = new();

        this.Root = new DummyElement()
        {
            Parent = root,
            ElementTypeName = name,
        }.AssociateWith(server);
        this.DynamicRoot = new DummyElement()
        {
            Parent = this.Root,
            ElementTypeName = name,
        }.AssociateWith(server);

        this.Exports = new List<string>();
    }

    public void Start()
    {
        var files = this.resourceProvider.GetFilesForResource(this);
        this.server.BroadcastPacket(new ResourceStartPacket(
            this.Name, this.NetId, this.Root.Id, this.DynamicRoot.Id, 0, null, null, false, this.PriorityGroup, files, this.Exports)
        );

        this.server.BroadcastPacket(new ResourceClientScriptsPacket(
            this.NetId, this.NoClientScripts.ToDictionary(x => x.Key, x => CompressFile(x.Value)))
        );
    }

    public void Stop()
    {
        this.server.BroadcastPacket(new ResourceStopPacket(this.NetId));
    }

    public void StartFor(Player player)
    {
        var files = this.resourceProvider.GetFilesForResource(this);
        new ResourceStartPacket(this.Name, this.NetId, this.Root.Id, this.DynamicRoot.Id, (ushort)this.NoClientScripts.Count, null, null, false, this.PriorityGroup, files, this.Exports)
            .SendTo(player);

        new ResourceClientScriptsPacket(this.NetId, this.NoClientScripts.ToDictionary(x => x.Key, x => CompressFile(x.Value)))
            .SendTo(player);
    }

    public void StopFor(Player player)
    {
        new ResourceStopPacket(this.NetId).SendTo(player);
    }

    private byte[] CompressFile(byte[] input)
    {
        var compressed = Ionic.Zlib.ZlibStream.CompressBuffer(input);

        var result = new byte[] {
                (byte)((input.Length >> 24) & 0xFF),
                (byte)((input.Length >> 8) & 0xFF),
                (byte)((input.Length >> 24) & 0xFF),
                (byte)(input.Length & 0xFF)
            }.Concat(compressed).ToArray();

        return result;
    }
}
