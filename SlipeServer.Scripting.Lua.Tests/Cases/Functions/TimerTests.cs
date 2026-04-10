using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class TimerTests
{
    [Theory]
    [ScriptingAutoDomainData]
    public void SetTimer_CreatesALiveTimer(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local t = setTimer(function() end, 1000, 1)
            assertPrint(tostring(isTimer(t)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void KillTimer_MakesTimerInvalid(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local t = setTimer(function() end, 1000, 1)
            killTimer(t)
            assertPrint(tostring(isTimer(t)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsTimer_ReturnsFalseForNil(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            assertPrint(tostring(isTimer(nil)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetTimerDetails_ReturnsInterval(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local t = setTimer(function() end, 2000, 5)
            local remaining, execsLeft, interval = getTimerDetails(t)
            assertPrint(tostring(interval))
            assertPrint(tostring(execsLeft))
            """);

        assertDataProvider.AssertPrints[0].Should().Be("2000");
        assertDataProvider.AssertPrints[1].Should().Be("5");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void IsTimerPaused_ReturnsFalseInitially(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local t = setTimer(function() end, 1000, 1)
            assertPrint(tostring(isTimerPaused(t)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTimerPaused_PausesTimer(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local t = setTimer(function() end, 1000, 1)
            setTimerPaused(t, true)
            assertPrint(tostring(isTimerPaused(t)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTimerPaused_ThenUnpause_UnpausesTimer(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local t = setTimer(function() end, 1000, 1)
            setTimerPaused(t, true)
            setTimerPaused(t, false)
            assertPrint(tostring(isTimerPaused(t)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void GetTimers_ReturnsCreatedTimers(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local t1 = setTimer(function() end, 1000, 0)
            local t2 = setTimer(function() end, 2000, 0)
            local timers = getTimers()
            assertPrint(tostring(#timers >= 2))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void ResetTimer_KeepsTimerAlive(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local t = setTimer(function() end, 1000, 1)
            resetTimer(t)
            assertPrint(tostring(isTimer(t)))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData]
    public void SetTimer_WithInfiniteExecutions_ReportsZeroExecsRemaining(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local t = setTimer(function() end, 1000, 0)
            local _, execsLeft, _ = getTimerDetails(t)
            assertPrint(tostring(execsLeft))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0");
    }
}
