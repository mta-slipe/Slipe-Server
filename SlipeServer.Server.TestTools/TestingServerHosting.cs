using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using SlipeServer.Hosting;
using SlipeServer.Server.Elements;
using SlipeServer.Server.ServerBuilders;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SlipeServer.Server.TestTools;

public class TestingServerHosting<TPlayer> : IDisposable where TPlayer : Player
{
    private readonly IHost host;

    public TestingServer<TPlayer> Server { get; }
    public IHost Host => this.host;

    public TestingServerHosting(
        Configuration configuration, 
        Action<HostApplicationBuilder>? applicationBuilder = null, 
        Action<ServerBuilder>? serverBuilder = null)
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();


        if (typeof(TPlayer) == typeof(Player) || typeof(TPlayer) == typeof(TestingPlayer))
        {
            builder.AddMtaServer<TestingServer<TPlayer>>(new TestingServer<TPlayer>(configuration, x =>
            {
                x.UseConfiguration(configuration);
                serverBuilder?.Invoke(x);
            }));
        }
        else
        {
            builder.AddCustomMtaServerWithDiSupport(serviceProvider =>
            {
                return new TestingServer<TPlayer>(serviceProvider, x =>
                {
                    x.UseConfiguration(configuration);
                    serverBuilder?.Invoke(x);
                });
            });
        }

        applicationBuilder?.Invoke(builder);

        Mock<ILogger> loggerMock = new();
        builder.Services.TryAddSingleton(loggerMock.Object);

        this.host = builder.Build();

        var tcs = new TaskCompletionSource();
        var lifecycle = this.Host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifecycle.ApplicationStarted.Register(tcs.SetResult);

        var _ = Task.Run(async () =>
        {
            try
            {
                await this.host.RunAsync();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        tcs.Task.Wait();

        this.Server = this.host.Services.GetRequiredService<TestingServer<TPlayer>>();
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

    public T GetRequiredService<T>() where T : class => this.host.Services.GetRequiredService<T>();
}

public class TestingServerHosting : TestingServerHosting<TestingPlayer>
{
    public TestingServerHosting(
        Configuration configuration, 
        Action<HostApplicationBuilder>? applicationBuilder = null, 
        Action<ServerBuilder> serverBuilder = null
    ) : base(configuration, applicationBuilder, serverBuilder)
    {

    }
}
