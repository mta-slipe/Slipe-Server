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

        // b + g * 256 + r * 256 * 256 + a * 256 * 256 * 256 = 255 + 0 + 0 + 0 = 255
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
}
