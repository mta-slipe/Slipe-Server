using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class BitTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void BitAnd_ReturnsCorrectResult(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(bitAnd(12, 10)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("8");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BitOr_ReturnsCorrectResult(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(bitOr(12, 10)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("14");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BitXor_ReturnsCorrectResult(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(bitXor(12, 10)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("6");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BitNot_FlipsAllBits(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(bitNot(0)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("4294967295");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BitTest_WhenBitIsSet_ReturnsTrue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(bitTest(12, 8)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BitTest_WhenBitIsNotSet_ReturnsFalse(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(bitTest(12, 1)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BitLShift_ShiftsLeft(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(bitLShift(1, 3)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("8");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BitRShift_ShiftsRight(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(bitRShift(8, 3)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BitArShift_PreservesSignBit(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(bitArShift(-8, 1)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("-4");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BitLRotate_RotatesLeft(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(bitLRotate(1, 1)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("2");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BitRRotate_RotatesRight(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(bitRRotate(2, 1)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BitExtract_ExtractsBits(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(bitExtract(0xFF, 4, 4)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("15");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BitReplace_ReplacesBits(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(bitReplace(0xFF, 0, 0, 4)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("240");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void BitAnd_MultipleArgs_ChainsAndCorrectly(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(bitAnd(15, 7, 3)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("3");
    }
}
