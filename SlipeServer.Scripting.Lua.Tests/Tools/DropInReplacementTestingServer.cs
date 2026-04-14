using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SlipeServer.DropInReplacement.MixedResources;
using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.Lua;
using SlipeServer.Server;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.ServerBuilders;
using SlipeServer.Server.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Tools;

/// <summary>
/// A minimal MtaServer configured with DropInReplacement services for integration testing,
/// using a lightweight mock net wrapper instead of the native net.dll.
/// </summary>
public class DropInReplacementTestingServer : MtaServer<LightTestPlayer>
{
    private readonly LightTestNetWrapper netWrapper;
    private ulong nextAddress;

    public DropInReplacementTestingServer(string resourceDirectory) : base(builder =>
    {
        builder.UseConfiguration(new Configuration
        {
            ResourceDirectory = resourceDirectory,
        });

        builder.ConfigureServices(services =>
        {
            var resourceServerMock = new Mock<IResourceServer>();
            services.AddSingleton(resourceServerMock.Object);

            services.AddLogging();
            services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ILogger<MtaServer>>());

            services.AddSingleton<IResourceProvider, DropInReplacementResourceProvider>();
            services.AddSingleton<DropInReplacementResourceService>();
            services.AddSingleton<IDropInReplacementResourceService>(
                sp => sp.GetRequiredService<DropInReplacementResourceService>());
            services.AddSingleton<IResourceService>(
                sp => sp.GetRequiredService<DropInReplacementResourceService>());
            services.AddSingleton<IDropInReplacementResourceLuaService, DropInReplacementResourceLuaService>();

            services.AddLua();
            services.AddHttpClient();
        });

        // Registers the DropInReplacement interpreter with the provider and triggers Refresh()
        builder.AddResourceInterpreter<DropInReplacementResourceInterpreter>();
    })
    {
        this.netWrapper = new LightTestNetWrapper();
        this.AddNetWrapper(this.netWrapper);
        this.clients.Add(this.netWrapper, []);
        this.GetRequiredService<LuaService>().ScriptErrored += message => ScriptErrors.Add(message);
    }

    public List<string> ScriptErrors { get; } = [];

    /// <summary>
    /// Creates a new player and fires the PlayerJoined event, simulating a full join.
    /// </summary>
    public LightTestPlayer JoinFakePlayer()
    {
        var player = new LightTestPlayer(this.netWrapper, this.nextAddress++)
        {
            Name = $"TestPlayer_{this.nextAddress}"
        };

        this.clients[this.netWrapper].Add(player.Address, player.Client);
        player.AssociateWith(this);
        this.HandlePlayerJoin(player);

        return player;
    }
}
