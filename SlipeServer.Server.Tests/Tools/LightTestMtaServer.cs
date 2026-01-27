using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SlipeServer.Net.Wrappers;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Resources.Serving;

namespace SlipeServer.Server.Tests.Tools;

public class LightTestMtaServer : MtaServer<LightTestPlayer>
{
    private readonly INetWrapper wrapper;
    private ulong address;

    public LightTestMtaServer(INetWrapper wrapper, IElementCollection elementCollection) : base(builder =>
    {
        builder.ConfigureServices(x => ConfigureOverrides(x, elementCollection));
    })
    {
        this.AddNetWrapper(wrapper);
        this.wrapper = wrapper;
    }

    public LightTestPlayer CreatePlayer()
    {
        return new LightTestPlayer(this.wrapper, this.address++)
        {
            Name = $"TestPlayer_{this.address}"
        };
    }


    public static void ConfigureOverrides(IServiceCollection services, IElementCollection elementCollection)
    {
        var httpServerMock = new Mock<IResourceServer>();
        services.AddSingleton<IResourceServer>(httpServerMock.Object);
        services.AddLogging();
        services.AddSingleton<ILogger>(x => x.GetRequiredService<ILogger<MtaServer>>());
        services.AddSingleton<IElementCollection>(elementCollection);
    }
}
