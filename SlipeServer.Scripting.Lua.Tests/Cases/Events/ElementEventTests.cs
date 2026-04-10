using AutoFixture.Xunit2;
using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Events;

public class ElementEventTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnElementDestroyed_FiresWhenElementIsDestroyed(
        [Frozen] IElementCollection _,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            addEventHandler("onElementDestroyed", object, function()
                assertPrint("destroyed")
            end)
            destroyElement(object)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("destroyed");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnElementDestroy_FiresWhenElementIsDestroyed(
        [Frozen] IElementCollection _,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            addEventHandler("onElementDestroy", object, function()
                assertPrint("destroyed")
            end)
            destroyElement(object)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("destroyed");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnElementDataChange_FiresWhenDataIsSet(
        [Frozen] IElementCollection _,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            addEventHandler("onElementDataChange", object, function(key, oldValue, newValue)
                assertPrint(key .. ":" .. tostring(oldValue) .. ":" .. tostring(newValue))
            end)
            setElementData(object, "testKey", "hello")
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("testKey:nil:hello");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnElementDataChange_FiresWithOldValueWhenDataIsUpdated(
        [Frozen] IElementCollection _,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            setElementData(object, "testKey", "first")
            addEventHandler("onElementDataChange", object, function(key, oldValue, newValue)
                assertPrint(key .. ":" .. tostring(oldValue) .. ":" .. tostring(newValue))
            end)
            setElementData(object, "testKey", "second")
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("testKey:first:second");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnElementDimensionChange_FiresWhenDimensionChanges(
        [Frozen] IElementCollection _,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            addEventHandler("onElementDimensionChange", object, function(oldDimension, newDimension)
                assertPrint(tostring(oldDimension) .. ":" .. tostring(newDimension))
            end)
            setElementDimension(object, 5)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0:5");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnElementInteriorChange_FiresWhenInteriorChanges(
        [Frozen] IElementCollection _,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            addEventHandler("onElementInteriorChange", object, function(oldInterior, newInterior)
                assertPrint(tostring(oldInterior) .. ":" .. tostring(newInterior))
            end)
            setElementInterior(object, 3)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0:3");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnElementModelChange_FiresWhenObjectModelChanges(
        [Frozen] IElementCollection _,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            addEventHandler("onElementModelChange", object, function(oldModel, newModel)
                assertPrint(tostring(oldModel) .. ":" .. tostring(newModel))
            end)
            setElementModel(object, 400)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("321:400");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnElementModelChange_FiresWhenPedModelChanges(
        [Frozen] IElementCollection _,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local ped = createPed(0, 0, 0, 0)
            addEventHandler("onElementModelChange", ped, function(oldModel, newModel)
                assertPrint(tostring(oldModel) .. ":" .. tostring(newModel))
            end)
            setElementModel(ped, 22)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("0:22");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnElementColShapeHit_FiresWhenElementEntersColShape(
        [Frozen] IElementCollection elementCollection,
        CollisionShapeBehaviour _,
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        player.Position = new Vector3(0, 0, 0);
        elementCollection.Add(player);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local colshape = createColSphere(10, 0, 0, 5)
            addEventHandler("onElementColShapeHit", testPlayer, function(colShape, matchingDimension)
                assertPrint("hit:" .. tostring(matchingDimension))
            end)
            """);

        player.Position = new Vector3(7, 0, 0);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hit:true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnElementColShapeLeave_FiresWhenElementLeavesColShape(
        [Frozen] IElementCollection elementCollection,
        CollisionShapeBehaviour _,
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        player.Position = new Vector3(0, 0, 0);
        elementCollection.Add(player);

        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local colshape = createColSphere(10, 0, 0, 5)
            addEventHandler("onElementColShapeLeave", testPlayer, function(colShape, matchingDimension)
                assertPrint("left:" .. tostring(matchingDimension))
            end)
            """);

        player.Position = new Vector3(7, 0, 0);
        player.Position = new Vector3(0, 0, 0);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("left:true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnElementStartSync_FiresWhenSyncerAssigned(
        [Frozen] IElementCollection _,
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local ped = createPed(0, 0, 0, 0)
            addEventHandler("onElementStartSync", ped, function(newSyncer)
                assertPrint("syncing:" .. getPlayerName(newSyncer))
            end)
            setElementSyncer(ped, testPlayer)
            """);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().StartWith("syncing:");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnElementStopSync_FiresWhenSyncerRemoved(
        [Frozen] IElementCollection elementCollection,
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.AddGlobal("testPlayer", player);

        sut.RunLuaScript("""
            local ped = createPed(0, 0, 0, 0)
            setElementSyncer(ped, testPlayer)
            addEventHandler("onElementStopSync", ped, function(oldSyncer)
                assertPrint("stopped")
            end)
            """);

        var ped = elementCollection.GetByType<Ped>().Single(x => x is not Player);
        ped.Syncer = null;

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("stopped");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnElementClicked_FiresWhenElementIsClicked(
        [Frozen] IElementCollection elementCollection,
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        sut.RunLuaScript("""
            local object = createObject(321, 3, 4, 5)
            addEventHandler("onElementClicked", object, function(button, state, clicker, wx, wy, wz)
                assertPrint(button .. ":" .. state .. ":" .. getPlayerName(clicker))
            end)
            """);

        var worldObject = elementCollection.GetByType<WorldObject>().Single();
        worldObject.TriggerClicked(new ElementClickedEventArgs(player, CursorButton.Left, true, new Vector3(1, 2, 3)));

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().StartWith("left:down:");
    }
}
