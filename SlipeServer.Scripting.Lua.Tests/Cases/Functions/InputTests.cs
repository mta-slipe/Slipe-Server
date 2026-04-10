using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Elements.Enums;
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

    [Theory]
    [ScriptingAutoDomainData]
    public void ExecuteCommandHandler_InvokesRegisteredHandler(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.HandlePlayerJoin(player);
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            addCommandHandler("ping", function(p, cmd)
                assertPrint("ping executed")
            end)
            executeCommandHandler("ping", testPlayer)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("ping executed");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetCommandHandlers_ReturnsRegisteredCommands(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addCommandHandler("foo", function() end)
            addCommandHandler("bar", function() end)
            local handlers = getCommandHandlers()
            local count = 0
            for _ in pairs(handlers) do count = count + 1 end
            assertPrint(tostring(count >= 2))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BindKey_DoesNotThrow(
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.HandlePlayerJoin(player);
        sut.AddGlobal("testPlayer", player);

        var act = () => sut.RunLuaScript("""
            bindKey(testPlayer, "a", "down", function(p, key, state) end)
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BindKey_InvokesCallbackWhenKeyPressed(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.HandlePlayerJoin(player);
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            bindKey(testPlayer, "a", "down", function(p, key, state)
                assertPrint(key .. ":" .. state)
            end)
            """);

        player.TriggerBoundKey(BindType.Function, KeyState.Down, "a");

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("a:down");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BindKey_BothState_InvokesOnDownAndUp(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.HandlePlayerJoin(player);
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            bindKey(testPlayer, "b", "both", function(p, key, state)
                assertPrint(state)
            end)
            """);

        player.TriggerBoundKey(BindType.Function, KeyState.Down, "b");
        player.TriggerBoundKey(BindType.Function, KeyState.Up, "b");

        assertDataProvider.AssertPrints.Should().HaveCount(2)
            .And.ContainInOrder("down", "up");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void UnbindKey_RemovesCallback(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.HandlePlayerJoin(player);
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local handler = function(p, key, state)
                assertPrint("fired")
            end
            bindKey(testPlayer, "c", "down", handler)
            unbindKey(testPlayer, "c", "down", handler)
            """);

        player.TriggerBoundKey(BindType.Function, KeyState.Down, "c");

        assertDataProvider.AssertPrints.Should().BeEmpty();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsKeyBound_ReturnsTrueWhenBound(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.HandlePlayerJoin(player);
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            bindKey(testPlayer, "d", "down", function() end)
            assertPrint(tostring(isKeyBound(testPlayer, "d")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsKeyBound_ReturnsFalseWhenNotBound(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.HandlePlayerJoin(player);
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            assertPrint(tostring(isKeyBound(testPlayer, "e")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetFunctionsBoundToKey_ReturnsRegisteredCallbacks(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.HandlePlayerJoin(player);
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            bindKey(testPlayer, "f", "down", function() end)
            bindKey(testPlayer, "f", "down", function() end)
            local funcs = getFunctionsBoundToKey(testPlayer, "f", "down")
            local count = 0
            for _ in pairs(funcs) do count = count + 1 end
            assertPrint(tostring(count))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetKeyBoundToFunction_ReturnsKeyName(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.HandlePlayerJoin(player);
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local handler = function() end
            bindKey(testPlayer, "g", "down", handler)
            assertPrint(getKeyBoundToFunction(testPlayer, handler))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("g");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsControlEnabled_ReturnsTrueByDefault(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.HandlePlayerJoin(player);
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            assertPrint(tostring(isControlEnabled(testPlayer, "fire")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ToggleControl_DisablesControl(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.HandlePlayerJoin(player);
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            toggleControl(testPlayer, "fire", false)
            assertPrint(tostring(isControlEnabled(testPlayer, "fire")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ToggleAllControls_DisablesAllControls(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.HandlePlayerJoin(player);
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            toggleAllControls(testPlayer, false)
            assertPrint(tostring(isControlEnabled(testPlayer, "jump")))
            assertPrint(tostring(isControlEnabled(testPlayer, "sprint")))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2)
            .And.AllBe("false");
    }
}
