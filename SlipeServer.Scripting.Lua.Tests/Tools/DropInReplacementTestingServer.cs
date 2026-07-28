using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using Moq;
using SlipeServer.DropInReplacement.MixedResources;
using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.Lua;
using SlipeServer.Scripting.Definitions;
using SlipeServer.Scripting.Lua.Tests.Mocks;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
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

    public DropInReplacementTestingServer(string resourceDirectory, IScriptTimerService? timerService = null)
        : this(new LightTestNetWrapper(), resourceDirectory, timerService) { }

    public DropInReplacementTestingServer(LightTestNetWrapper netWrapper, string resourceDirectory, IScriptTimerService? timerService = null) : base(builder =>
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
            services.AddSingleton<DropInReplacementResourceService>(sp =>
                new DropInReplacementResourceService(
                    sp.GetRequiredService<IMtaServer>(),
                    sp.GetRequiredService<IResourceProvider>(),
                    sp.GetRequiredService<ILogger<DropInReplacementResourceProvider>>(),
                    sp.GetRequiredService<IDropInReplacementResourceLuaService>(),
                    allowMissingIncludes: true));
            services.AddSingleton<IDropInReplacementResourceService>(
                sp => sp.GetRequiredService<DropInReplacementResourceService>());
            services.AddSingleton<IResourceService>(
                sp => sp.GetRequiredService<DropInReplacementResourceService>());
            services.AddSingleton<IDropInReplacementResourceLuaService, DropInReplacementResourceLuaService>();

            if (timerService != null)
            {
                services.AddSingleton(timerService);
                services.AddSingleton<IScriptTimerService>(timerService);
            }

            services.AddSingleton<MockSqlExecutor>();
            services.AddSingleton<ISqlExecutor>(sp => sp.GetRequiredService<MockSqlExecutor>());

            services.AddLua();
            services.AddHttpClient();
        });

        // Registers the DropInReplacement interpreter with the provider and triggers Refresh()
        builder.AddResourceInterpreter<DropInReplacementResourceInterpreter>();
    })
    {
        this.netWrapper = netWrapper;
        this.AddNetWrapper(this.netWrapper);
        this.clients.Add(this.netWrapper, []);
        this.GetRequiredService<LuaService>().ScriptErrored += message => ScriptErrors.Add(message);
    }

    public List<string> ScriptErrors { get; } = [];
    public MockSqlExecutor SqlExecutor => this.GetRequiredService<MockSqlExecutor>();
    public LightTestNetWrapper NetWrapper => this.netWrapper;
    public RootElement GetRootElement() => this.RootElement;

    /// <summary>
    /// Calls an exported Lua function from a resource and returns the first result cast to <typeparamref name="T"/>.
    /// </summary>
    public T? CallLuaExport<T>(string resourceName, string functionName, params object[] args)
    {
        var envService = this.GetRequiredService<LuaEnvironmentService>();
        var env = envService.GetAllEnvironments()
            .FirstOrDefault(e => e.ExecutionContext.Owner?.Name == resourceName);

        if (env == null)
            throw new InvalidOperationException($"No Lua environment found for resource '{resourceName}'");

        var results = env.CallFunction(functionName, args);
        if (results.Length == 0)
            return default;

        var first = results[0];
        if (first.Type == DataType.Boolean)
            return (T?)(object)first.Boolean;
        if (first.Type == DataType.Nil || first.IsNil())
            return default;
        if (first.Type == DataType.Number)
        {
            if (typeof(T) == typeof(long) || typeof(T) == typeof(long?))
                return (T?)(object)(long)first.Number;
            if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
                return (T?)(object)(int)first.Number;
            return (T?)(object)first.Number;
        }
        if (first.Type == DataType.String)
            return (T?)(object)first.String;
        if (first.UserData?.Object is T typed)
            return typed;
        return (T?)first.ToObject();
    }

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
        player.Parent = this.RootElement;
        this.HandlePlayerJoin(player);

        return player;
    }
}
