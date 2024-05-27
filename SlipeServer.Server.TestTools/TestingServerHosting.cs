using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SlipeServer.Hosting;
using SlipeServer.Server.Elements;
using SlipeServer.Server.ServerBuilders;
using System;
using System.Threading;

namespace SlipeServer.Server.TestTools;

public class TestingServerHosting<T> : IDisposable where T : Player
{
    private readonly IHost host;

    public TestingServer<T> Server => this.host.Services.GetRequiredService<TestingServer<T>>();
    public IHost Host => this.host;

    public TestingServerHosting(Configuration configuration, Func<IServiceProvider, TestingServer<T>> serverFactory, Action<HostApplicationBuilder>? applicationBuilder = null, Action<ServerBuilder>? serverBuilder = null)
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();

        builder.Services.AddMtaServer<TestingServer<T>>(configuration, serverFactory, builder =>
        {
            builder.AddDefaultServices();
            serverBuilder?.Invoke(builder);
        });

        applicationBuilder?.Invoke(builder);
        this.host = builder.Build();

        this.host.RunAsync();
    }

    public void Dispose()
    {
        var waitHandle = new AutoResetEvent(false);
        this.Server.Stopped += () =>
        {
            waitHandle.Set();
        };
        this.host.StopAsync().Wait();
        waitHandle.WaitOne(TimeSpan.FromSeconds(30));
    }
}

public class TestingServerHosting : TestingServerHosting<TestingPlayer>
{
    public TestingServerHosting(Configuration configuration, Action<HostApplicationBuilder>? applicationBuilder = null, Action<ServerBuilder>? serverBuilder = null) : base(configuration, services => new TestingServer<TestingPlayer>(services, configuration), applicationBuilder, serverBuilder)
    {

    }
}
