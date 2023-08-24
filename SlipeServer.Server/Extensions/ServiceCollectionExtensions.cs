using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SlipeServer.Net.Wrappers;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Elements;
using SlipeServer.Server.ServerBuilders;
using System;

namespace SlipeServer.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddMtaServer(this ServiceCollection serviceCollection, Action<ServerBuilder> builderAction, Configuration configuration,
        Func<uint, INetWrapper, IClient>? clientCreationMethod = null)
    {
        serviceCollection.AddSingleton(x => new MtaServer(builderAction, clientCreationMethod, serviceCollection, configuration).Init());
    }

    public static void AddMtaServer<TPlayer>(this ServiceCollection serviceCollection, Action<ServerBuilder> builderAction, Configuration configuration) where TPlayer: Player
    {
        serviceCollection.AddSingleton(x => MtaServer.CreateWithDiSupport<TPlayer>(builderAction, serviceCollection, configuration).Init());
    }
}
