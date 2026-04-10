using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class SettingsRegistryTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void Get_NonExistentKey_ReturnsNil(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(get("nonexistent")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("nil");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Set_ReturnsTrue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local result = set("myKey", "hello")
            assertPrint(tostring(result))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetAndGet_StringValue_RoundTrips(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            set("myKey", "hello world")
            assertPrint(get("myKey"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hello world");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetAndGet_NumberValue_RoundTrips(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            set("myNumber", 42)
            assertPrint(tostring(get("myNumber")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("42");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetAndGet_BooleanValue_RoundTrips(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            set("myBool", true)
            assertPrint(tostring(get("myBool")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Set_OverwritesExistingValue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            set("key", "first")
            set("key", "second")
            assertPrint(get("key"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("second");
    }
}
