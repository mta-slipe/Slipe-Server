using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SlipeServer.Lua;
using SlipeServer.Net.Wrappers;
using SlipeServer.Scripting.Definitions;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.Services;
using SlipeServer.Server.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Tools;

public class ScriptingTestMtaServer : MtaServer<LightTestPlayer>
{
    private readonly INetWrapper wrapper;
    private ulong address;

    public ScriptingTestMtaServer(INetWrapper wrapper, IElementCollection elementCollection, ScriptingAssertDefinitions definitions, ITextItemService textItemService) : base(builder =>
    {
        builder.ConfigureServices(x => ConfigureOverrides(x, elementCollection, textItemService));
    })
    {
        this.AddNetWrapper(wrapper);
        this.wrapper = wrapper;

        this.clients.Add(wrapper, []);

        this.GetRequiredService<IScriptEventRuntime>().LoadDefaultEvents();
        this.GetRequiredService<LuaService>().LoadDefaultDefinitions();
        this.GetRequiredService<LuaService>().LoadDefinitions(definitions);
    }

    public LightTestPlayer CreatePlayer()
    {
        var player =  new LightTestPlayer(this.wrapper, this.address++)
        {
            Name = $"TestPlayer_{this.address}"
        };

        this.clients[this.wrapper].Add(player.Address, player.Client);
        player.AssociateWith(this);
        player.Client.FetchSerial();
        player.Client.FetchIp();

        return player;
    }


    public static void ConfigureOverrides(IServiceCollection services, IElementCollection elementCollection, ITextItemService textItemService)
    {
        var httpServerMock = new Mock<IResourceServer>();
        services.AddSingleton<IResourceServer>(httpServerMock.Object);
        services.AddLogging();
        services.AddSingleton<ILogger>(x => x.GetRequiredService<ILogger<MtaServer>>());
        services.AddSingleton<IElementCollection>(elementCollection);

        services.AddSingleton<IAccountService>(new SqliteAccountService(":memory:"));
        services.AddScripting();
        services.AddLua();
        services.AddSingleton<ITextItemService>(textItemService);
    }
}
