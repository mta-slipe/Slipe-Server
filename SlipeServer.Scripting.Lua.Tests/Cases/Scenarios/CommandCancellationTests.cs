using AutoFixture.Xunit2;
using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Scenarios;

/// <summary>
/// Verifies that calling cancelEvent() inside an onPlayerCommand handler prevents
/// addCommandHandler callbacks from executing, on both specific-player and root element.
/// The script must be loaded before the player joins so that the onPlayerCommand proxy
/// subscribes to player.CommandEntered before ScriptInputRuntime.CommandEntered does
/// (guaranteed by the DI-enforced construction order of ScriptEventRuntime before ScriptInputRuntime).
/// </summary>
public class CommandCancellationTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void CancelEvent_InOnPlayerCommand_OnSpecificPlayer_PreventsCommandHandler(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerCommand", testPlayer, function(cmd)
                cancelEvent()
            end)

            addCommandHandler("test", function(p, cmd)
                assertPrint("handler called")
            end)
            """);

        sut.HandlePlayerJoin(player);
        player.TriggerCommand("test", []);

        assertDataProvider.AssertPrints.Should().BeEmpty();
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void CancelEvent_InOnPlayerCommand_OnRoot_PreventsCommandHandler(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEventHandler("onPlayerCommand", root, function(cmd)
                cancelEvent()
            end)

            addCommandHandler("test", function(p, cmd)
                assertPrint("handler called")
            end)
            """);

        sut.HandlePlayerJoin(player);
        player.TriggerCommand("test", []);

        assertDataProvider.AssertPrints.Should().BeEmpty();
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void NoCancelEvent_InOnPlayerCommand_OnSpecificPlayer_AllowsCommandHandler(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addEventHandler("onPlayerCommand", testPlayer, function(cmd)
                assertPrint("event fired")
            end)

            addCommandHandler("test", function(p, cmd)
                assertPrint("handler called")
            end)
            """);

        sut.HandlePlayerJoin(player);
        player.TriggerCommand("test", []);

        assertDataProvider.AssertPrints.Should().HaveCount(2)
            .And.ContainInOrder("event fired", "handler called");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void NoCancelEvent_InOnPlayerCommand_OnRoot_AllowsCommandHandler(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEventHandler("onPlayerCommand", root, function(cmd)
                assertPrint("event fired")
            end)

            addCommandHandler("test", function(p, cmd)
                assertPrint("handler called")
            end)
            """);

        sut.HandlePlayerJoin(player);
        player.TriggerCommand("test", []);

        assertDataProvider.AssertPrints.Should().HaveCount(2)
            .And.ContainInOrder("event fired", "handler called");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void CancelEvent_InOnPlayerCommand_DoesNotAffectOtherCommands(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEventHandler("onPlayerCommand", root, function(cmd)
                if cmd == "cancel" then
                    cancelEvent()
                end
            end)

            addCommandHandler("cancel", function(p, cmd)
                assertPrint("cancel handler called")
            end)

            addCommandHandler("allow", function(p, cmd)
                assertPrint("allow handler called")
            end)
            """);

        sut.HandlePlayerJoin(player);
        player.TriggerCommand("allow", []);
        player.TriggerCommand("cancel", []);

        assertDataProvider.AssertPrints.Should().ContainSingle()
            .Which.Should().Be("allow handler called");
    }
}
