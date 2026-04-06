using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class InputTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void AddCommandHandler_DoesNotThrow(IMtaServer sut)
    {
        var act = () => sut.RunLuaScript("""
            addCommandHandler("hello", function(player, command)
            end)
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void RemoveCommandHandler_DoesNotThrow(IMtaServer sut)
    {
        var act = () => sut.RunLuaScript("""
            local handler = function(player, command) end
            addCommandHandler("hello", handler)
            removeCommandHandler("hello", handler)
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void AddCommandHandler_InvokesCallbackWhenCommandTriggered(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.HandlePlayerJoin(player);

        sut.RunLuaScript("""
            addCommandHandler("greet", function(player, command)
                assertPrint("greet called")
            end)
            """);

        player.TriggerCommand("greet", []);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("greet called");
    }
}
