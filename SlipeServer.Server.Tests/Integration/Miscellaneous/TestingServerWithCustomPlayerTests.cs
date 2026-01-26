using FluentAssertions;
using SlipeServer.Net.Wrappers;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Elements;
using SlipeServer.Server.ServerBuilders;
using SlipeServer.Server.TestTools;
using System;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Miscellaneous;

public class TestCustomPlayer : Player
{

}

public class TestingServerWithCustomPlayer(Configuration configuration = null, Action<ServerBuilder>? configure = null) : TestingServer<TestCustomPlayer>(configuration, configure)
{
    protected override IClient CreateClient(ulong binaryAddress, INetWrapper netWrapper)
    {
        var player = new TestCustomPlayer();
        player.Client = new TestingClient(binaryAddress, netWrapper, player);
        return player.Client;
    }
}

public class TestingServerWithCustomPlayerTests
{
    [Fact]
    public void TestServerShouldSupportCustomPlayersImplementations()
    {
        var testingServer = new TestingServerWithCustomPlayer();
        var testingPlayer = testingServer.AddFakePlayer();

        testingPlayer.Should().BeOfType<TestCustomPlayer>();
    }
}
