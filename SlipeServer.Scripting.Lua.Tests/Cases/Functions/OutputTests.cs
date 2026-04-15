using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class OutputTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void OutputDebugString_DoesNotThrow(IMtaServer sut)
    {
        var act = () => sut.RunLuaScript("""
            outputDebugString("test debug message")
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void OutputServerLog_DoesNotThrow(IMtaServer sut)
    {
        var act = () => sut.RunLuaScript("""
            outputServerLog("test server log message")
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void OutputChatBox_DoesNotThrow(IMtaServer sut)
    {
        var act = () => sut.RunLuaScript("""
            outputChatBox("Hello, world!")
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void OutputChatBox_ToPlayer_DoesNotThrow(
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        var act = () => sut.RunLuaScript("""
            outputChatBox("Hello player!", testPlayer)
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ClearChatBox_DoesNotThrow(IMtaServer sut)
    {
        var act = () => sut.RunLuaScript("""
            clearChatBox()
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ClearChatBox_ForPlayer_DoesNotThrow(
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        var act = () => sut.RunLuaScript("""
            clearChatBox(testPlayer)
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ShowChat_DoesNotThrow(
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        var act = () => sut.RunLuaScript("""
            showChat(testPlayer, false)
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ShowChat_WithInputBlocked_DoesNotThrow(
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        var act = () => sut.RunLuaScript("""
            showChat(testPlayer, false, true)
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void OutputChatBox_WithRootElement_DoesNotThrow(IMtaServer sut)
    {
        var act = () => sut.RunLuaScript("""
            outputChatBox("Hello everyone!", root)
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void OutputChatBox_WithRootElementAndColor_DoesNotThrow(IMtaServer sut)
    {
        var act = () => sut.RunLuaScript("""
            outputChatBox("Hello colored!", root, 255, 127, 0)
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void OutputChatBox_WithPlayerTable_DoesNotThrow(
        LightTestPlayer player1,
        LightTestPlayer player2,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer1", player1);
        sut.AddGlobal("testPlayer2", player2);

        var act = () => sut.RunLuaScript("""
            outputChatBox("Hello selected!", {testPlayer1, testPlayer2}, 255, 0, 0)
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void OutputChatBox_WithPlayerAndColor_DoesNotThrow(
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        var act = () => sut.RunLuaScript("""
            outputChatBox("Hello player!", testPlayer, 255, 0, 0)
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void OutputConsole_DoesNotThrow(IMtaServer sut)
    {
        var act = () => sut.RunLuaScript("""
            outputConsole("hello console")
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void OutputConsole_ToPlayer_DoesNotThrow(
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        var act = () => sut.RunLuaScript("""
            outputConsole("hello player console", testPlayer)
            """);

        act.Should().NotThrow();
    }
}
