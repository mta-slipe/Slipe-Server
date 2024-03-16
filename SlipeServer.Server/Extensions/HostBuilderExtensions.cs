using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SlipeServer.Server.Elements;
using SlipeServer.Server.ServerBuilders;
using System;

namespace SlipeServer.Server.Extensions;

public static class HostBuilderExtensions
{
    public static IHostApplicationBuilder ConfigureMtaServer(this IHostApplicationBuilder host, Action<ServerBuilder> builder)
    {
        host.Services.AddSingleton(MtaServer.Create(builder, host.Services, false));
        host.Services.AddHostedService<MtaServerHostingService>();
        return host;
    }

    public static IHostApplicationBuilder ConfigureMtaServer<TPlayer>(this IHostApplicationBuilder host, Action<ServerBuilder> builder) where TPlayer : Player
    {
        var server = MtaServer.CreateWithDiSupport<TPlayer>(builder, host.Services, false);
        host.Services.AddSingleton(server);
        host.Services.AddHostedService<MtaServerHostingService>();
        return host;
    }
}
