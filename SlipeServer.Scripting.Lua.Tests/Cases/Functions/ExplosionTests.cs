using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class ExplosionTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void CreateExplosion_ReturnsTrueForValidType(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local result = createExplosion(0, 0, 0, 0)
            assertPrint(tostring(result))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void CreateExplosion_ReturnsFalseForInvalidType(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local result = createExplosion(0, 0, 0, 999)
            assertPrint(tostring(result))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }
}
