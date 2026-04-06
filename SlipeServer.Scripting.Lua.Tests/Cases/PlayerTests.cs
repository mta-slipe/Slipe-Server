using AutoFixture.Xunit2;
using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Tests.Tools;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class PlayerTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void GetPlayerName_ReturnsPlayerName(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            assertPrint(getPlayerName(testPlayer))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be(player.Name);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPlayerMoney_ReturnsZeroByDefault(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            assertPrint(tostring(getPlayerMoney(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPlayerMoney_UpdatesMoney(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setPlayerMoney(testPlayer, 1000)
            assertPrint(tostring(getPlayerMoney(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1000");
        player.Money.Should().Be(1000);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TakePlayerMoney_ReducesMoney(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setPlayerMoney(testPlayer, 500)
            takePlayerMoney(testPlayer, 200)
            assertPrint(tostring(getPlayerMoney(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("300");
        player.Money.Should().Be(300);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GiveMoney_IncreasesMoney(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setPlayerMoney(testPlayer, 100)
            giveMoney(testPlayer, 400)
            assertPrint(tostring(getPlayerMoney(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("500");
        player.Money.Should().Be(500);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TryTakePlayerMoney_ReturnsTrueWhenSufficientFunds(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setPlayerMoney(testPlayer, 500)
            assertPrint(tostring(tryTakePlayerMoney(testPlayer, 200)))
            assertPrint(tostring(getPlayerMoney(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("300");
        player.Money.Should().Be(300);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TryTakePlayerMoney_ReturnsFalseWhenInsufficientFunds(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setPlayerMoney(testPlayer, 100)
            assertPrint(tostring(tryTakePlayerMoney(testPlayer, 500)))
            assertPrint(tostring(getPlayerMoney(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("false");
        assertDataProvider.AssertPrints[1].Should().Be("100");
        player.Money.Should().Be(100);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPlayerWantedLevel_ReturnsZeroByDefault(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            assertPrint(tostring(getPlayerWantedLevel(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPlayerWantedLevel_UpdatesWantedLevel(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setPlayerWantedLevel(testPlayer, 3)
            assertPrint(tostring(getPlayerWantedLevel(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("3");
        player.WantedLevel.Should().Be(3);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsPlayerMuted_ReturnsFalseByDefault(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            assertPrint(tostring(isPlayerMuted(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPlayerMuted_UpdatesMutedState(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setPlayerMuted(testPlayer, true)
            assertPrint(tostring(isPlayerMuted(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        player.IsChatMuted.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPlayerBlurLevel_ReturnsDefaultBlurLevel(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            assertPrint(tostring(getPlayerBlurLevel(testPlayer)))
            """);

        // Default Player.BlurLevel is 36
        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("36");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPlayerBlurLevel_UpdatesBlurLevel(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setPlayerBlurLevel(testPlayer, 10)
            assertPrint(tostring(getPlayerBlurLevel(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("10");
        player.BlurLevel.Should().Be(10);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPlayerNametagText_ReturnsPlayerNameByDefault(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            assertPrint(getPlayerNametagText(testPlayer))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be(player.Name);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPlayerNametagText_UpdatesNametagText(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setPlayerNametagText(testPlayer, "CustomTag")
            assertPrint(getPlayerNametagText(testPlayer))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("CustomTag");
        player.NametagText.Should().Be("CustomTag");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsPlayerMapForced_ReturnsFalseByDefault(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            assertPrint(tostring(isPlayerMapForced(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ForcePlayerMap_UpdatesMapForcedState(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            forcePlayerMap(testPlayer, true)
            assertPrint(tostring(isPlayerMapForced(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        player.IsMapForced.Should().BeTrue();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetPlayerScriptDebugLevel_ReturnsZeroByDefault(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            assertPrint(tostring(getPlayerScriptDebugLevel(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetPlayerScriptDebugLevel_UpdatesDebugLevel(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setPlayerScriptDebugLevel(testPlayer, 2)
            assertPrint(tostring(getPlayerScriptDebugLevel(testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
        player.DebugLogLevel.Should().Be(2);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetControlState_ReturnsFalseForUnsetControl(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            assertPrint(tostring(getControlState(testPlayer, "forward")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetControlState_UpdatesControlState(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            setControlState(testPlayer, "forward", true)
            assertPrint(tostring(getControlState(testPlayer, "forward")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
        player.Controls.IsControlStateSet("forward").Should().BeTrue();
    }
}
