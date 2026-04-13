using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Events;

public class ServerEventTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnExplosion_BothHandlersFire_WhenTwoHandlersRegistered(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local handler1 = function(x, y, z, type) assertPrint("handler1") end
            local handler2 = function(x, y, z, type) assertPrint("handler2") end
            addEventHandler("onExplosion", getRootElement(), handler1)
            addEventHandler("onExplosion", getRootElement(), handler2)
            """);

        player.TriggerExplosionCreated(Vector3.Zero, ExplosionType.Grenade);

        assertDataProvider.AssertPrints.Should().HaveCount(2)
            .And.Contain("handler1")
            .And.Contain("handler2");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnExplosion_RemainingHandlerStillFires_AfterOneHandlerRemoved(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local handler1 = function(x, y, z, type) assertPrint("handler1") end
            local handler2 = function(x, y, z, type) assertPrint("handler2") end
            addEventHandler("onExplosion", getRootElement(), handler1)
            addEventHandler("onExplosion", getRootElement(), handler2)
            removeEventHandler("onExplosion", getRootElement(), handler1)
            """);

        player.TriggerExplosionCreated(Vector3.Zero, ExplosionType.Grenade);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("handler2");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnExplosion_RemovedHandlerDoesNotFire_AfterRemoval(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local handler1 = function(x, y, z, type) assertPrint("handler1") end
            local handler2 = function(x, y, z, type) assertPrint("handler2") end
            addEventHandler("onExplosion", getRootElement(), handler1)
            addEventHandler("onExplosion", getRootElement(), handler2)
            removeEventHandler("onExplosion", getRootElement(), handler2)
            """);

        player.TriggerExplosionCreated(Vector3.Zero, ExplosionType.Grenade);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("handler1");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnExplosion_ServiceExplosion_RemainingHandlerStillFires_AfterOneHandlerRemoved(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local handler1 = function(x, y, z, type) assertPrint("handler1") end
            local handler2 = function(x, y, z, type) assertPrint("handler2") end
            addEventHandler("onExplosion", getRootElement(), handler1)
            addEventHandler("onExplosion", getRootElement(), handler2)
            removeEventHandler("onExplosion", getRootElement(), handler1)
            """);

        sut.GetRequiredService<IExplosionService>().CreateExplosion(Vector3.Zero, ExplosionType.Grenade);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("handler2");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnChatMessage_BothHandlersFire_WhenTwoHandlersRegistered(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local handler1 = function(msg, type) assertPrint("handler1:" .. msg) end
            local handler2 = function(msg, type) assertPrint("handler2:" .. msg) end
            addEventHandler("onChatMessage", getRootElement(), handler1)
            addEventHandler("onChatMessage", getRootElement(), handler2)
            """);

        player.TriggerCommand("say", ["hello"]);

        assertDataProvider.AssertPrints.Should().HaveCount(2)
            .And.Contain("handler1:hello")
            .And.Contain("handler2:hello");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnChatMessage_RemainingHandlerStillFires_AfterOneHandlerRemoved(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local handler1 = function(msg, type) assertPrint("handler1:" .. msg) end
            local handler2 = function(msg, type) assertPrint("handler2:" .. msg) end
            addEventHandler("onChatMessage", getRootElement(), handler1)
            addEventHandler("onChatMessage", getRootElement(), handler2)
            removeEventHandler("onChatMessage", getRootElement(), handler1)
            """);

        player.TriggerCommand("say", ["hello"]);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("handler2:hello");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnChatMessage_RemovedHandlerDoesNotFire_AfterRemoval(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local handler1 = function(msg, type) assertPrint("handler1:" .. msg) end
            local handler2 = function(msg, type) assertPrint("handler2:" .. msg) end
            addEventHandler("onChatMessage", getRootElement(), handler1)
            addEventHandler("onChatMessage", getRootElement(), handler2)
            removeEventHandler("onChatMessage", getRootElement(), handler2)
            """);

        player.TriggerCommand("say", ["hello"]);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("handler1:hello");
    }
}
