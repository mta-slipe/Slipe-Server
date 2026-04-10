using AutoFixture.Xunit2;
using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Events;

public class PickupEventTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPickupHit_FiresWhenPlayerEntersPickup(
        [Frozen] IElementCollection elementCollection,
        CollisionShapeBehaviour _,
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        player.Position = new Vector3(100, 0, 0);
        elementCollection.Add(player);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local pickup = createPickup(0, 0, 0, 3, 1212, 0)
            addEventHandler("onPickupHit", pickup, function(player, matchingDimension)
                if player == testPlayer then
                    assertPrint("hit:" .. tostring(matchingDimension))
                end
            end)
            """);

        player.Position = new Vector3(0, 0, 0);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hit:true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPickupLeave_FiresWhenPlayerLeavesPickup(
        [Frozen] IElementCollection elementCollection,
        CollisionShapeBehaviour _,
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        player.Position = new Vector3(100, 0, 0);
        elementCollection.Add(player);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local pickup = createPickup(0, 0, 0, 3, 1212, 0)
            addEventHandler("onPickupLeave", pickup, function(player, matchingDimension)
                if player == testPlayer then
                    assertPrint("left:" .. tostring(matchingDimension))
                end
            end)
            """);

        player.Position = new Vector3(0, 0, 0);
        player.Position = new Vector3(100, 0, 0);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("left:true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPickupUse_FiresWhenPlayerUsesPickup(
        [Frozen] IElementCollection elementCollection,
        CollisionShapeBehaviour _,
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        player.Position = new Vector3(100, 0, 0);
        elementCollection.Add(player);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local pickup = createPickup(0, 0, 0, 3, 1212, 0)
            addEventHandler("onPickupUse", pickup, function(player)
                if player == testPlayer then
                    assertPrint("used")
                end
            end)
            """);

        player.Position = new Vector3(0, 0, 0);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("used");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerPickupHit_FiresOnPlayerWhenEnteringPickup(
        [Frozen] IElementCollection elementCollection,
        CollisionShapeBehaviour _,
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        player.Position = new Vector3(100, 0, 0);
        elementCollection.Add(player);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local pickup = createPickup(0, 0, 0, 3, 1212, 0)
            addEventHandler("onPlayerPickupHit", testPlayer, function(pickup, matchingDimension)
                assertPrint("hit:" .. tostring(matchingDimension))
            end)
            """);

        player.Position = new Vector3(0, 0, 0);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hit:true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerPickupLeave_FiresOnPlayerWhenLeavingPickup(
        [Frozen] IElementCollection elementCollection,
        CollisionShapeBehaviour _,
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        player.Position = new Vector3(100, 0, 0);
        elementCollection.Add(player);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local pickup = createPickup(0, 0, 0, 3, 1212, 0)
            addEventHandler("onPlayerPickupLeave", testPlayer, function(pickup, matchingDimension)
                assertPrint("left:" .. tostring(matchingDimension))
            end)
            """);

        player.Position = new Vector3(0, 0, 0);
        player.Position = new Vector3(100, 0, 0);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("left:true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerPickupUse_FiresOnPlayerWhenUsingPickup(
        [Frozen] IElementCollection elementCollection,
        CollisionShapeBehaviour _,
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        player.Position = new Vector3(100, 0, 0);
        elementCollection.Add(player);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local pickup = createPickup(0, 0, 0, 3, 1212, 0)
            addEventHandler("onPlayerPickupUse", testPlayer, function(pickup)
                assertPrint("used")
            end)
            """);

        player.Position = new Vector3(0, 0, 0);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("used");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPickupSpawn_FiresWhenPickupIsSpawned(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local pickup = createPickup(0, 0, 0, 3, 1212, 0)
            addEventHandler("onPickupSpawn", pickup, function()
                assertPrint("spawned")
            end)
            spawnPickup(pickup)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("spawned");
    }
}
