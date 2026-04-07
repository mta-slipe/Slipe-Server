using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class UtilityTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void Base64Encode_ReturnsEncodedString(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(base64Encode("hello"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("aGVsbG8=");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Base64Decode_ReturnsDecodedString(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(base64Decode("aGVsbG8="))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hello");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Base64Encode_ThenDecode_RoundTrips(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local encoded = base64Encode("round trip test")
            assertPrint(base64Decode(encoded))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("round trip test");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ToColor_ReturnsCorrectIntegerValue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(tocolor(0, 0, 255, 0)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("255");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetTickCount_ReturnsNonNegativeValue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getTickCount() >= 0))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Md5_ReturnsCorrectHash(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(md5("hello"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("5D41402ABC4B2A76B9719D911017C592");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Sha256_ReturnsCorrectHash(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(sha256("hello"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2CF24DBA5FB0A30E26E83B2AC5B9E29E1B161E5C1FA7425E73043362938B9824");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetDevelopmentMode_DefaultIsFalse(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getDevelopmentMode()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetDevelopmentMode_TogglesMode(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            setDevelopmentMode(true)
            assertPrint(tostring(getDevelopmentMode()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetFPSLimit_ReturnsDefault(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getFPSLimit()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("60");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetFPSLimit_ValidValue_ReturnsTrue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(setFPSLimit(90)))
            assertPrint(tostring(getFPSLimit()))
            """);

        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("90");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetFPSLimit_InvalidValue_ReturnsFalse(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(setFPSLimit(10)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsTransferBoxVisible_DefaultIsTrue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(isTransferBoxVisible()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTransferBoxVisible_ChangesVisibility(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            setTransferBoxVisible(false)
            assertPrint(tostring(isTransferBoxVisible()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsOOPEnabled_ReturnsFalse(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(isOOPEnabled()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetVersion_ReturnsTableWithExpectedFields(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local v = getVersion()
            assertPrint(v["name"])
            assertPrint(v["type"])
            """);

        assertDataProvider.AssertPrints[0].Should().Be("Slipe Server");
        assertDataProvider.AssertPrints[1].Should().Be("server");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetRealTime_ReturnsTableWithTimestampField(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local t = getRealTime()
            assertPrint(tostring(type(t) == "table"))
            assertPrint(tostring(t["timestamp"] > 0))
            """);

        assertDataProvider.AssertPrints[0].Should().Be("true");
        assertDataProvider.AssertPrints[1].Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetRealTime_WithTimestamp_ReturnsCorrectYear(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local t = getRealTime(946684800, false)
            assertPrint(tostring(t["year"]))
            assertPrint(tostring(t["month"]))
            assertPrint(tostring(t["monthday"]))
            """);

        assertDataProvider.AssertPrints[0].Should().Be("100");
        assertDataProvider.AssertPrints[1].Should().Be("0");
        assertDataProvider.AssertPrints[2].Should().Be("1");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Ref_ThenDeref_ReturnsSameValue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local id = ref("hello")
            assertPrint(deref(id))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hello");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Deref_InvalidRef_ReturnsNil(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local v = deref(99999)
            assertPrint(tostring(v == nil))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Inspect_StringValue_ReturnsJson(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(inspect("hello"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("\"hello\"");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetNetworkStats_ReturnsTableWithBytesReceived(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local stats = getNetworkStats()
            assertPrint(tostring(stats["bytesReceived"]))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetProcessMemoryStats_ReturnsTableWithMemoryFields(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local stats = getProcessMemoryStats()
            assertPrint(tostring(stats["residentMemorySize"] > 0))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetColorFromString_ValidColor_ReturnsRGBA(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local r, g, b, a = getColorFromString("#FF0000")
            assertPrint(tostring(r))
            assertPrint(tostring(g))
            assertPrint(tostring(b))
            """);

        assertDataProvider.AssertPrints[0].Should().Be("255");
        assertDataProvider.AssertPrints[1].Should().Be("0");
        assertDataProvider.AssertPrints[2].Should().Be("0");
    }
}
