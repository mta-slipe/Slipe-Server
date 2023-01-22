using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.ServerBuilders;
using SlipeServer.Server.TestTools;
using System;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Server;
public class TestingServerCustomBuilderStepTests
{
    [Fact]
    public void TestIfCustomBuilderStepIsWorking()
    {
        var server = new TestingServer(null, Configure);

        server.GetRequiredService<Func<int>>()().Should().Be(2);
    }

    private void Configure(ServerBuilder serverBuilder)
    {
        serverBuilder.ConfigureServices(services =>
        {
            services.AddSingleton(() => 2);
        });
    }
}
