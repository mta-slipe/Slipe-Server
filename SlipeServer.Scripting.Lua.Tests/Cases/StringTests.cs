using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class StringTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void Split_ByString_ReturnsTokens(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local parts = split("hello world foo", " ")
            assertPrint(tostring(#parts))
            assertPrint(parts[1])
            assertPrint(parts[2])
            assertPrint(parts[3])
            """);

        assertDataProvider.AssertPrints[0].Should().Be("3");
        assertDataProvider.AssertPrints[1].Should().Be("hello");
        assertDataProvider.AssertPrints[2].Should().Be("world");
        assertDataProvider.AssertPrints[3].Should().Be("foo");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Split_ByCharCode_ReturnsTokens(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local parts = split("a,b,c", 44)
            assertPrint(tostring(#parts))
            assertPrint(parts[1])
            assertPrint(parts[3])
            """);

        assertDataProvider.AssertPrints[0].Should().Be("3");
        assertDataProvider.AssertPrints[1].Should().Be("a");
        assertDataProvider.AssertPrints[2].Should().Be("c");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetTok_ReturnsCorrectToken(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(gettok("a,b,c", 2, ","))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("b");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void PregFind_WhenMatches_ReturnsTrue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(pregFind("hello world", "w[a-z]+")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void PregFind_WhenNoMatch_ReturnsFalse(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(pregFind("hello", "xyz")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void PregMatch_ReturnsAllMatches(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local matches = pregMatch("hello world", "[a-z]+")
            assertPrint(tostring(#matches))
            assertPrint(matches[1])
            assertPrint(matches[2])
            """);

        assertDataProvider.AssertPrints[0].Should().Be("2");
        assertDataProvider.AssertPrints[1].Should().Be("hello");
        assertDataProvider.AssertPrints[2].Should().Be("world");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void PregReplace_ReplacesPattern(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(pregReplace("hello world", "world", "earth"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hello earth");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void UtfLen_ReturnsCharacterCount(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(utfLen("hello")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("5");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void UtfChar_ReturnsCharacterFromCode(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(utfChar(65))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("A");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void UtfCode_ReturnsCodePointOfCharacter(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(utfCode("A")))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("65");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void UtfSub_ExtractsSubstring(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(utfSub("hello", 2, 4))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("ell");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void TeaEncode_ThenDecode_RoundTrips(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local encoded = teaEncode("secret message", "mykey")
            assertPrint(teaDecode(encoded, "mykey"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("secret message");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void EncodeString_Base64_ReturnsBase64(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(encodeString("base64", "hello"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("aGVsbG8=");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void DecodeString_Base64_ReturnsOriginal(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(decodeString("base64", "aGVsbG8="))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hello");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void EncodeString_Tea_ThenDecode_RoundTrips(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local enc = encodeString("tea", "round trip", {key="testkey"})
            assertPrint(decodeString("tea", enc, {key="testkey"}))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("round trip");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ToJson_StringValue_ProducesJson(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(toJSON("hello"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("\"hello\"");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FromJson_ParsesStringValue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local val = fromJSON('"hello"')
            assertPrint(val)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hello");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void FromJson_ParsesTableValue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local t = fromJSON('{"name":"test"}')
            assertPrint(t["name"])
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("test");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ToJson_ThenFromJson_RoundTrips(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local original = {name = "alice", score = 42}
            local json = toJSON(original)
            local restored = fromJSON(json)
            assertPrint(restored["name"])
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("alice");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void Hash_Sha256_ReturnsCorrectHash(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(hash("sha256", "hello"))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should()
            .Be("2CF24DBA5FB0A30E26E83B2AC5B9E29E1B161E5C1FA7425E73043362938B9824");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void PasswordHash_Bcrypt_CanBeVerified(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local h = passwordHash("secret", "bcrypt", {cost=4})
            assertPrint(tostring(passwordVerify("secret", h)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void PasswordVerify_WrongPassword_ReturnsFalse(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local h = passwordHash("secret", "bcrypt", {cost=4})
            assertPrint(tostring(passwordVerify("wrong", h)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }
}
