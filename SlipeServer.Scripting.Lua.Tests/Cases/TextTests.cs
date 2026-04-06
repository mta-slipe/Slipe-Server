using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Tests.Tools;
using System.Drawing;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class TextTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void TextCreateTextItem_CreatesTextItem(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local item = textCreateTextItem("Hello", 0.5, 0.5)
            assertPrint(textItemGetText(item))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("Hello");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextItemGetText_ReturnsCorrectText(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local item = textCreateTextItem("SomeText", 0.1, 0.2)
            assertPrint(textItemGetText(item))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("SomeText");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextItemSetText_UpdatesText(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local item = textCreateTextItem("Old", 0.5, 0.5)
            textItemSetText(item, "New")
            assertPrint(textItemGetText(item))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("New");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextItemGetPosition_ReturnsCorrectPosition(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local item = textCreateTextItem("Test", 0.3, 0.7)
            local x, y = textItemGetPosition(item)
            assertPrint(string.format("%.1f,%.1f", x, y))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0.3,0.7");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextItemSetPosition_UpdatesPosition(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local item = textCreateTextItem("Test", 0.1, 0.2)
            textItemSetPosition(item, 0.6, 0.8)
            local x, y = textItemGetPosition(item)
            assertPrint(string.format("%.1f,%.1f", x, y))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0.6,0.8");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextItemGetScale_ReturnsDefaultScale(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local item = textCreateTextItem("Test", 0.5, 0.5)
            assertPrint(tostring(textItemGetScale(item)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextItemSetScale_UpdatesScale(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local item = textCreateTextItem("Test", 0.5, 0.5)
            textItemSetScale(item, 2.0)
            assertPrint(tostring(textItemGetScale(item)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextItemGetColor_ReturnsDefaultWhite(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local item = textCreateTextItem("Test", 0.5, 0.5)
            local r, g, b, a = textItemGetColor(item)
            assertPrint(r .. "," .. g .. "," .. b .. "," .. a)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("255,255,255,255");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextItemSetColor_UpdatesColor(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local item = textCreateTextItem("Test", 0.5, 0.5)
            textItemSetColor(item, 100, 150, 200, 128)
            local r, g, b, a = textItemGetColor(item)
            assertPrint(r .. "," .. g .. "," .. b .. "," .. a)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("100,150,200,128");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextItemGetPriority_ReturnsDefaultMedium(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local item = textCreateTextItem("Test", 0.5, 0.5)
            assertPrint(tostring(textItemGetPriority(item)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextItemSetPriority_UpdatesPriority(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local item = textCreateTextItem("Test", 0.5, 0.5)
            textItemSetPriority(item, "high")
            assertPrint(tostring(textItemGetPriority(item)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextCreateDisplay_CreatesDisplay(
        IMtaServer sut)
    {
        var act = () => sut.RunLuaScript("""
            local display = textCreateDisplay()
            assert(display ~= nil)
            """);

        act.Should().NotThrow();
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextDisplayIsObserver_ReturnsFalseByDefault(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local display = textCreateDisplay()
            assertPrint(tostring(textDisplayIsObserver(display, testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextDisplayAddObserver_MakesPlayerObserver(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local display = textCreateDisplay()
            textDisplayAddObserver(display, testPlayer)
            assertPrint(tostring(textDisplayIsObserver(display, testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextDisplayRemoveObserver_RemovesPlayer(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local display = textCreateDisplay()
            textDisplayAddObserver(display, testPlayer)
            textDisplayRemoveObserver(display, testPlayer)
            assertPrint(tostring(textDisplayIsObserver(display, testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextDisplayGetObservers_ReturnsObservers(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local display = textCreateDisplay()
            textDisplayAddObserver(display, testPlayer)
            local observers = textDisplayGetObservers(display)
            assertPrint(tostring(#observers))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TextDestroyDisplay_DestroysDisplay(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local display = textCreateDisplay()
            textDisplayAddObserver(display, testPlayer)
            textDestroyDisplay(display)
            assertPrint(tostring(textDisplayIsObserver(display, testPlayer)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }
}
