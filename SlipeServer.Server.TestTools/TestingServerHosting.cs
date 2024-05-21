using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SlipeServer.Hosting;
using SlipeServer.Server.ServerBuilders;
using System;
using System.Threading;

namespace SlipeServer.Server.TestTools;

public class TestingServerHosting<T> : IDisposable where T : TestingPlayer
{
    private readonly TestingServer<T> server;
    private readonly IHost host;
    private readonly CancellationTokenSource cancellationTokenSource;

    public TestingServer<T> Server => this.server;
    public IHost Host => this.host;

    public TestingServerHosting(Action<HostApplicationBuilder>? applicationBuilder = null, Action<ServerBuilder>? serverBuilder = null)
    {
        this.cancellationTokenSource = new();
        this.server = new TestingServer<T>();
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();

        builder.Services.AddDefaultMtaServerServices();

        builder.Services.AddHostedService<DefaultStartAllMtaServersHostedService>();

        builder.Services.AddMtaServer(this.server, builder =>
        {
            builder.AddDefaultServices();
            serverBuilder?.Invoke(builder);
        });

        applicationBuilder?.Invoke(builder);
        this.host = builder.Build();

        this.host.RunAsync(this.cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        var waitHandle = new AutoResetEvent(false);
        this.server.Stopped += () =>
        {
            waitHandle.Set();
        };
        this.cancellationTokenSource.Cancel();
        waitHandle.WaitOne(TimeSpan.FromSeconds(30));
    }
}

public class TestingServerHosting : TestingServerHosting<TestingPlayer>
{
    public TestingServerHosting(Action<HostApplicationBuilder>? applicationBuilder = null, Action<ServerBuilder>? serverBuilder = null) : base(applicationBuilder, serverBuilder)
    {

    }
}
