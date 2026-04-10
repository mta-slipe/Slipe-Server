using AutoFixture.Xunit2;
using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Scripting.Lua.Tests.Cases;

public class EventTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void AddEvent_AndTriggerEvent_InvokesHandler(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEvent("onMyCustomEvent", false)
            addEventHandler("onMyCustomEvent", root, function(msg)
                assertPrint(msg)
            end)
            triggerEvent("onMyCustomEvent", root, "hello from event")
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hello from event");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void TriggerEvent_WithMultipleArguments_PassesAllArguments(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEvent("onMultiArgEvent", false)
            addEventHandler("onMultiArgEvent", root, function(a, b, c)
                assertPrint(tostring(a) .. "," .. tostring(b) .. "," .. tostring(c))
            end)
            triggerEvent("onMultiArgEvent", root, "foo", 42, true)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("foo,42,true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void CancelEvent_WasEventCancelled_ReturnsTrue(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEvent("onCancelableEvent", false)
            addEventHandler("onCancelableEvent", root, function()
                cancelEvent()
            end)
            triggerEvent("onCancelableEvent", root)
            assertPrint(tostring(wasEventCancelled()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void CancelEvent_WithReason_GetCancelReasonReturnsReason(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEvent("onReasonEvent", false)
            addEventHandler("onReasonEvent", root, function()
                cancelEvent(true, "not allowed")
            end)
            triggerEvent("onReasonEvent", root)
            assertPrint(getCancelReason())
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("not allowed");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void WasEventCancelled_WhenNotCancelled_ReturnsFalse(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEvent("onUncancelledEvent", false)
            addEventHandler("onUncancelledEvent", root, function() end)
            triggerEvent("onUncancelledEvent", root)
            assertPrint(tostring(wasEventCancelled()))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("false");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void GetEventHandlers_ReturnsRegisteredHandlers(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEvent("onHandlerCheckEvent", false)
            local function myHandler() end
            addEventHandler("onHandlerCheckEvent", root, myHandler)
            local handlers = getEventHandlers("onHandlerCheckEvent", root)
            assertPrint(tostring(#handlers))
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("1");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void TriggerEvent_PropagatesFromChildToParent(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            addEvent("onPropagationEvent", false)
            addEventHandler("onPropagationEvent", root, function()
                assertPrint("received on root")
            end)
            local child = createElement("mytype")
            triggerEvent("onPropagationEvent", child)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("received on root");
    }
}
