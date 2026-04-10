using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class ServerTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void GetServerName_ReturnsConfiguredName(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(getServerName())
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be(sut.Configuration.ServerName);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetServerPort_ReturnsConfiguredPort(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getServerPort()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be(sut.Configuration.Port.ToString());
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetServerHttpPort_ReturnsConfiguredHttpPort(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getServerHttpPort()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be(sut.Configuration.HttpPort.ToString());
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetMaxPlayers_ReturnsConfiguredMaxPlayers(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getMaxPlayers()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be(sut.Configuration.MaxPlayerCount.ToString());
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetMaxPlayers_UpdatesMaxPlayerCount(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local result = setMaxPlayers(32)
            assertPrint(tostring(result))
            assertPrint(tostring(getMaxPlayers()))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("32");
        sut.Configuration.MaxPlayerCount.Should().Be(32);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetServerPassword_ReturnsNilWhenNoPassword(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getServerPassword()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("nil");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetServerPassword_UpdatesPassword(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local result = setServerPassword("secret123")
            assertPrint(tostring(result))
            assertPrint(getServerPassword())
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("secret123");
        sut.Password.Should().Be("secret123");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetServerConfigSetting_ReturnsServerName(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(getServerConfigSetting("servername"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be(sut.Configuration.ServerName);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetServerConfigSetting_ReturnsMaxPlayers(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(getServerConfigSetting("maxplayers"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be(sut.Configuration.MaxPlayerCount.ToString());
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetServerConfigSetting_UpdatesServerName(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local result = setServerConfigSetting("servername", "My New Server")
            assertPrint(tostring(result))
            assertPrint(getServerName())
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("My New Server");
        sut.Configuration.ServerName.Should().Be("My New Server");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsGlitchEnabled_ReturnsFalseByDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(isGlitchEnabled("fastmove")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetGlitchEnabled_EnablesGlitch(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local result = setGlitchEnabled("fastmove", true)
            assertPrint(tostring(result))
            assertPrint(tostring(isGlitchEnabled("fastmove")))
            """);

        assertDataProvider.AssertPrints.Should().HaveCount(2);
        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetGlitchEnabled_DisablesGlitch(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            setGlitchEnabled("quickreload", true)
            setGlitchEnabled("quickreload", false)
            assertPrint(tostring(isGlitchEnabled("quickreload")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetGlitchEnabled_ReturnsFalseForUnknownGlitch(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local result = setGlitchEnabled("nonexistentglitch", true)
            assertPrint(tostring(result))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetServerIpFromMasterServer_ReturnsHostString(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(getServerIpFromMasterServer())
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be(sut.Configuration.MasterServerHost);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Shutdown_DoesNotThrow(IMtaServer sut)
    {
        var act = () => sut.RunLuaScript("""
            shutdown("test shutdown")
            """);

        act.Should().NotThrow();
    }
}
