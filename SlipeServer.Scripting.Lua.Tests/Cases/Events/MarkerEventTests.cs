using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Events;

public class MarkerEventTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnMarkerHit_FiresWhenElementHitsMarker(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var marker = new Marker(Vector3.Zero, MarkerType.Checkpoint).AssociateWith(sut);
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut);

        sut.AddGlobal("testMarker", marker);

        sut.RunLuaScript("""
            addEventHandler("onMarkerHit", testMarker, function(hitElement, matchingDimension)
                assertPrint("hit:" .. tostring(matchingDimension))
            end)
            """);

        marker.ColShape!.CheckElementWithin(ped);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("hit:true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnMarkerLeave_FiresWhenElementLeavesMarker(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var marker = new Marker(Vector3.Zero, MarkerType.Checkpoint).AssociateWith(sut);
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut);

        sut.AddGlobal("testMarker", marker);

        sut.RunLuaScript("""
            addEventHandler("onMarkerLeave", testMarker, function(leftElement, matchingDimension)
                assertPrint("leave:" .. tostring(matchingDimension))
            end)
            """);

        marker.ColShape!.CheckElementWithin(ped);
        ped.Position = new Vector3(100, 0, 0);
        marker.ColShape.CheckElementWithin(ped);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("leave:true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerMarkerHit_FiresWhenPlayerHitsMarker(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var marker = new Marker(Vector3.Zero, MarkerType.Checkpoint).AssociateWith(sut);

        sut.AddGlobal("testPlayer", player);
        sut.AddGlobal("testMarker", marker);

        sut.RunLuaScript("""
            addEventHandler("onPlayerMarkerHit", testPlayer, function(markerHit, matchingDimension)
                assertPrint("markerhit:" .. tostring(matchingDimension))
            end)
            """);

        player.TriggerMarkerHit(marker, true);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("markerhit:true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnPlayerMarkerLeave_FiresWhenPlayerLeavesMarker(
        LightTestPlayer player,
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var marker = new Marker(Vector3.Zero, MarkerType.Checkpoint).AssociateWith(sut);

        sut.AddGlobal("testPlayer", player);
        sut.AddGlobal("testMarker", marker);

        sut.RunLuaScript("""
            addEventHandler("onPlayerMarkerLeave", testPlayer, function(markerLeft, matchingDimension)
                assertPrint("markerleave:" .. tostring(matchingDimension))
            end)
            """);

        player.TriggerMarkerLeft(marker, false);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("markerleave:false");
    }
}
