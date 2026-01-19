using SlipeServer.Packets.Definitions.Resources;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlipeServer.Server.Resources;

/// <summary>
/// Represents a client-side Lua resource
/// </summary>
public class Resource
{
    private readonly MtaServer server;
    private readonly Dictionary<string, byte[]> noClientScripts = [];

    public DummyElement Root { get; }
    public DummyElement DynamicRoot { get; }
    public ushort NetId { get; set; }
    public int PriorityGroup { get; set; }
    public List<string> Exports { get; init; }
    public List<ResourceFile> Files { get; init; }
    public IReadOnlyDictionary<string, byte[]> NoClientScripts => this.noClientScripts.AsReadOnly();
    public string Name { get; }
    public string Path { get; }
    public bool IsOopEnabled { get; set; }

    public Resource(
        MtaServer server, 
        RootElement root, 
        string name, 
        string? path = null
    )
    {
        this.server = server;
        this.Name = name;
        this.Path = path ?? $"./{name}";

        this.Files = new();

        this.Root = new DummyElement()
        {
            Parent = root,
            ElementTypeName = name,
        }.AssociateWith(server);
        this.DynamicRoot = new DummyElement()
        {
            Parent = this.Root,
            ElementTypeName = "map",
        }.AssociateWith(server);

        this.Exports = [];
    }

    public void AddNoClientScript(string name, string source)
    {
        AddNoClientScript(name, Encoding.UTF8.GetBytes(source));
    }

    public void AddNoClientScript(string name, byte[] source)
    {
        if (source.Length == 0)
            return;

        if (this.noClientScripts.ContainsKey(name))
            throw new System.ArgumentException($"A client script with the name '{name}' already exists in the collection.", nameof(name));

        this.noClientScripts[name] = CompressFile(source);
    }

    public void Start()
    {
        this.server.BroadcastPacket(new ResourceStartPacket(
            this.Name, this.NetId, this.Root.Id, this.DynamicRoot.Id, (ushort)this.noClientScripts.Count, null, null, this.IsOopEnabled, this.PriorityGroup, this.Files, this.Exports)
        );

        this.server.BroadcastPacket(new ResourceClientScriptsPacket(
            this.NetId, this.noClientScripts)
        );
    }
    
    public void Stop()
    {
        this.server.BroadcastPacket(new ResourceStopPacket(this.NetId));
    }

    public void StartFor(Player player)
    {
        new ResourceStartPacket(this.Name, this.NetId, this.Root.Id, this.DynamicRoot.Id, (ushort)this.noClientScripts.Count, null, null, this.IsOopEnabled, this.PriorityGroup, this.Files, this.Exports)
            .SendTo(player);

        if (this.noClientScripts.Any())
            new ResourceClientScriptsPacket(this.NetId, this.noClientScripts)
                .SendTo(player);
    }

    public Task StartForAsync(Player player, CancellationToken cancelationToken = default)
    {
        cancelationToken.ThrowIfCancellationRequested();

        var source = new TaskCompletionSource();

        cancelationToken.Register(() =>
        {
            player.ResourceStarted -= HandleResourceStart;
            player.Disconnected -= HandlePlayerDisconnected;
        });

        player.ResourceStarted += HandleResourceStart;
        player.Disconnected += HandlePlayerDisconnected;

        void HandleResourceStart(Player sender, PlayerResourceStartedEventArgs e)
        {
            if (e.NetId != this.NetId)
                return;

            player.ResourceStarted -= HandleResourceStart;
            player.Disconnected -= HandlePlayerDisconnected;

            source.SetResult();
        }

        void HandlePlayerDisconnected(Player disconnectingPlayer, PlayerQuitEventArgs e)
        {
            if(player != disconnectingPlayer)
                return;

            player.ResourceStarted -= HandleResourceStart;
            player.Disconnected -= HandlePlayerDisconnected;

            source.SetException(new System.Exception("Player disconnected."));
        }

        StartFor(player);

        return source.Task;
    }

    public void StopFor(Player player)
    {
        new ResourceStopPacket(this.NetId).SendTo(player);
    }

    public static byte[] CompressFile(byte[] input)
    {
        using var output = new System.IO.MemoryStream();
        using (var compressor = new System.IO.Compression.DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal, true))
        {
            compressor.Write(input, 0, input.Length);
        }
        var compressed = output.ToArray();

        var result = new byte[] {
                (byte)((input.Length >> 24) & 0xFF),
                (byte)((input.Length >> 8) & 0xFF),
                (byte)((input.Length >> 24) & 0xFF),
                (byte)(input.Length & 0xFF)
            }.Concat(compressed).ToArray();

        return result;
    }
}
