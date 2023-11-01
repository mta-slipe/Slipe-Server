using FluentAssertions;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.TestTools;
using System.Linq;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Miscellaneous;

public class TestingServerEvents
{
    [Fact]
    public void TestingServerTestEvents()
    {
        var testingServer = new TestingServer();
        var testingPlayer = testingServer.AddFakePlayer();

        testingPlayer.TriggerLuaEvent("test event", testingPlayer, 1, "two");
        var sendEvents = testingServer.GetSendLuaEvents().ToList();
        var sendEvent = sendEvents.First();

        sendEvent.Address.Should().Be(testingPlayer.GetAddress());
        sendEvent.Name.Should().Be("test event");
        sendEvent.Source.Should().Be(testingPlayer.Id);
        sendEvent.Arguments.Should().BeEquivalentTo(new LuaValue[] { 1, "two" });
        testingServer.VerifyLuaEventTriggered("test event", testingPlayer, testingPlayer, new LuaValue[] { 1, "two" }, 1);
    }
}
