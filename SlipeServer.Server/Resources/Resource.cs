using SlipeServer.Packets.Definitions.Resources;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlipeServer.Server.Resources;

/// <summary>
/// Represents a client-side Lua resource
/// </summary>
public class Resource : IResource
{
    private readonly IMtaServer server;

    public DummyElement Root { get; init; }
    Element IResource.Root => this.Root;
    public DummyElement DynamicRoot { get; init; }
    public ushort NetId { get; set; }
    public int PriorityGroup { get; set; }
    public List<string> Exports { get; init; } = [];
    public List<ResourceFile> Files { get; init; } = [];

    private readonly Dictionary<string, byte[]> noClientScripts = [];

    /// <summary>
    /// The client scripts of this resource that the client is not allowed to cache, keyed by file name.
    /// The values are the zlib compressed script sources, exactly as they are sent to the client.
    /// </summary>
    public IReadOnlyDictionary<string, byte[]> NoClientScripts => this.noClientScripts.AsReadOnly();

    public string Name { get; }
    public string Path { get; }
    public bool IsOopEnabled { get; set; }
    public Dictionary<string, string> Info { get; init; } = [];

    /// <summary>
    /// Minimum MTA server version this resource requires, from the <c>min_mta_version</c> element of its
    /// meta.xml. Null when the resource does not specify a requirement.
    /// </summary>
    public string? MinServerVersion { get; set; }

    /// <summary>
    /// Minimum MTA client version this resource requires, from the <c>min_mta_version</c> element of its
    /// meta.xml. Null when the resource does not specify a requirement.
    /// </summary>
    public string? MinClientVersion { get; set; }

    public Resource(
        IMtaServer server, 
        IRootElement root, 
        string name, 
        string? path = null
    )
    {
        this.server = server;
        this.Name = name;
        this.Path = path ?? $"./{name}";


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
    }

    /// <summary>
    /// Adds a client script that the client is not allowed to cache. The script source is compressed once,
    /// here, so that starting the resource or a player joining does not recompress it every time.
    /// Zero length sources are ignored.
    /// </summary>
    /// <exception cref="ArgumentException">A script with the same name was already added.</exception>
    public void AddNoClientScript(string name, string source) =>
        this.AddNoClientScript(name, Encoding.UTF8.GetBytes(source));

    /// <inheritdoc cref="AddNoClientScript(string, string)"/>
    public void AddNoClientScript(string name, byte[] source)
    {
        if (source.Length == 0)
            return;

        if (this.noClientScripts.ContainsKey(name))
            throw new ArgumentException($"A client script with the name '{name}' already exists in the collection.", nameof(name));

        this.noClientScripts[name] = CompressFile(source);
    }

    public void Start()
    {
        this.server.BroadcastPacket(new ResourceStartPacket(
            this.Name, this.NetId, this.Root.Id, this.DynamicRoot.Id, (ushort)this.noClientScripts.Count, this.MinServerVersion, this.MinClientVersion, this.IsOopEnabled, this.PriorityGroup, this.Files, this.Exports)
        );

        this.server.BroadcastPacket(new ResourceClientScriptsPacket(this.NetId, this.noClientScripts));
    }

    public void Stop()
    {
        this.server.BroadcastPacket(new ResourceStopPacket(this.NetId));
    }

    public void StartFor(Player player)
    {
        new ResourceStartPacket(this.Name, this.NetId, this.Root.Id, this.DynamicRoot.Id, (ushort)this.noClientScripts.Count, this.MinServerVersion, this.MinClientVersion, this.IsOopEnabled, this.PriorityGroup, this.Files, this.Exports)
            .SendTo(player);

        if (this.noClientScripts.Count > 0)
            new ResourceClientScriptsPacket(this.NetId, this.noClientScripts)
                .SendTo(player);
    }

    public Task StartForAsync(Player player, CancellationToken cancelationToken = default)
    {
        cancelationToken.ThrowIfCancellationRequested();

        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cleanupDone = 0;
        var cancellationRegistration = new CancellationTokenRegistration();

        void Cleanup()
        {
            if (Interlocked.Exchange(ref cleanupDone, 1) != 0)
                return;

            player.ResourceStarted -= HandleResourceStart;
            player.Disconnected -= HandlePlayerDisconnected;
            cancellationRegistration.Dispose();
        }

        cancellationRegistration = cancelationToken.Register(() =>
        {
            Cleanup();
            source.TrySetCanceled(cancelationToken);
        });

        player.ResourceStarted += HandleResourceStart;
        player.Disconnected += HandlePlayerDisconnected;

        void HandleResourceStart(Player sender, PlayerResourceStartedEventArgs e)
        {
            if (e.NetId != this.NetId)
                return;

            Cleanup();
            source.TrySetResult();
        }

        void HandlePlayerDisconnected(Player disconnectingPlayer, PlayerQuitEventArgs e)
        {
            if(player != disconnectingPlayer)
                return;

            Cleanup();
            source.TrySetException(new PlayerQuitDuringResourceStartException(player));
        }

        StartFor(player);

        return source.Task;
    }

    public async Task<bool> TryStartForAsync(Player player, CancellationToken cancelationToken = default)
    {
        try
        {
            await StartForAsync(player, cancelationToken);
            return true;
        } 
        catch (PlayerQuitDuringResourceStartException)
        {
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void StopFor(Player player)
    {
        new ResourceStopPacket(this.NetId).SendTo(player);
    }

    /// <summary>
    /// Resolves the effective server and client version requirements from the attributes of MTA's
    /// <c>min_mta_version</c> element, where <c>both</c> takes precedence over the individual
    /// <c>server</c> and <c>client</c> attributes.
    /// </summary>
    public static (string? Server, string? Client) ResolveMinMtaVersion(string? server, string? client, string? both) =>
        both is not null ? (both, both) : (server, client);

    public static byte[] CompressFile(byte[] input)
    {
        using var output = new MemoryStream();
        using (var compressor = new ZLibStream(output, CompressionLevel.Optimal, true))
        {
            compressor.Write(input, 0, input.Length);
        }
        var compressed = output.ToArray();

        var result = new byte[] {
                (byte)((input.Length >> 24) & 0xFF),
                (byte)((input.Length >> 16) & 0xFF),
                (byte)((input.Length >> 8) & 0xFF),
                (byte)(input.Length & 0xFF)
            }.Concat(compressed).ToArray();

        return result;
    }
}

public class PlayerQuitDuringResourceStartException(Player player) : Exception($"Player {player.Name} disconnected during resource start.") { }
