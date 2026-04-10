using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class MathTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void GetDistanceBetweenPoints2D_ReturnsCorrectDistance(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getDistanceBetweenPoints2D(0, 0, 3, 4)))
            """);

        float.Parse(assertDataProvider.AssertPrints[0]).Should().BeApproximately(5f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetDistanceBetweenPoints2D_SamePoint_ReturnsZero(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getDistanceBetweenPoints2D(5, 5, 5, 5)))
            """);

        float.Parse(assertDataProvider.AssertPrints[0]).Should().BeApproximately(0f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetDistanceBetweenPoints3D_ReturnsCorrectDistance(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getDistanceBetweenPoints3D(0, 0, 0, 1, 1, 1)))
            """);

        float.Parse(assertDataProvider.AssertPrints[0]).Should().BeApproximately(1.732f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetEasingValue_Linear_ReturnsProgress(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getEasingValue(0.5, "Linear")))
            """);

        float.Parse(assertDataProvider.AssertPrints[0]).Should().BeApproximately(0.5f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetEasingValue_LinearAtZero_ReturnsZero(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getEasingValue(0, "Linear")))
            """);

        float.Parse(assertDataProvider.AssertPrints[0]).Should().BeApproximately(0f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetEasingValue_LinearAtOne_ReturnsOne(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getEasingValue(1, "Linear")))
            """);

        float.Parse(assertDataProvider.AssertPrints[0]).Should().BeApproximately(1f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetEasingValue_InQuad_ReturnsSquaredProgress(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(getEasingValue(0.5, "InQuad")))
            """);

        float.Parse(assertDataProvider.AssertPrints[0]).Should().BeApproximately(0.25f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void InterpolateBetween_Linear_MidpointIsHalf(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local x, y, z = interpolateBetween(0, 0, 0, 10, 10, 10, 0.5, "Linear")
            assertPrint(tostring(x))
            assertPrint(tostring(y))
            assertPrint(tostring(z))
            """);

        float.Parse(assertDataProvider.AssertPrints[0]).Should().BeApproximately(5f, 0.001f);
        float.Parse(assertDataProvider.AssertPrints[1]).Should().BeApproximately(5f, 0.001f);
        float.Parse(assertDataProvider.AssertPrints[2]).Should().BeApproximately(5f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void InterpolateBetween_Linear_AtZeroReturnsStart(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local x, y, z = interpolateBetween(1, 2, 3, 10, 20, 30, 0, "Linear")
            assertPrint(tostring(x))
            assertPrint(tostring(y))
            assertPrint(tostring(z))
            """);

        float.Parse(assertDataProvider.AssertPrints[0]).Should().BeApproximately(1f, 0.001f);
        float.Parse(assertDataProvider.AssertPrints[1]).Should().BeApproximately(2f, 0.001f);
        float.Parse(assertDataProvider.AssertPrints[2]).Should().BeApproximately(3f, 0.001f);
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void InterpolateBetween_Linear_AtOneReturnsEnd(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local x, y, z = interpolateBetween(1, 2, 3, 10, 20, 30, 1, "Linear")
            assertPrint(tostring(x))
            assertPrint(tostring(y))
            assertPrint(tostring(z))
            """);

        float.Parse(assertDataProvider.AssertPrints[0]).Should().BeApproximately(10f, 0.001f);
        float.Parse(assertDataProvider.AssertPrints[1]).Should().BeApproximately(20f, 0.001f);
        float.Parse(assertDataProvider.AssertPrints[2]).Should().BeApproximately(30f, 0.001f);
    }
}
