using FluentAssertions;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Events;

public class VehicleEventTests
{
    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnVehicleEnter_FiresWhenPedWarpsIn(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(sut);
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut);

        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            addEventHandler("onVehicleEnter", testVehicle, function(ped, seat, jacked)
                assertPrint("enter:" .. tostring(seat))
            end)
            """);

        ped.WarpIntoVehicle(vehicle);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("enter:0");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnVehicleExit_FiresWhenPedExitsVehicle(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(sut);
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut);
        ped.WarpIntoVehicle(vehicle);

        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            addEventHandler("onVehicleExit", testVehicle, function(ped, seat, jacker, forcedByScript)
                assertPrint("exit:" .. tostring(seat))
            end)
            """);

        ped.RemoveFromVehicle();

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("exit:0");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnVehicleStartEnter_FiresWhenPedStartsEntering(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(sut);
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut);

        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            addEventHandler("onVehicleStartEnter", testVehicle, function(enteringPed, seat, jacked, door)
                assertPrint("start-enter:" .. tostring(seat))
            end)
            """);

        vehicle.TriggerPedStartedEntering(ped, 0, null, 0);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("start-enter:0");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnVehicleStartExit_FiresWhenPedStartsExiting(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(sut);
        var ped = new Ped(PedModel.Cj, Vector3.Zero).AssociateWith(sut);
        ped.WarpIntoVehicle(vehicle);

        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            addEventHandler("onVehicleStartExit", testVehicle, function(exitingPed, seat, jacker, door)
                assertPrint("start-exit:" .. tostring(seat))
            end)
            """);

        vehicle.TriggerPedStartedExiting(ped, 0, null, 0);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("start-exit:0");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnVehicleExplode_FiresWhenVehicleBlownUp(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(sut);

        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            addEventHandler("onVehicleExplode", testVehicle, function()
                assertPrint("explode")
            end)
            """);

        vehicle.BlowUp();

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("explode");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnVehicleRespawn_FiresWithExplodedTrueWhenPreviouslyBlown(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(sut);
        vehicle.BlowUp();

        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            addEventHandler("onVehicleRespawn", testVehicle, function(exploded)
                assertPrint("respawn:" .. tostring(exploded))
            end)
            """);

        vehicle.Respawn();

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("respawn:true");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnVehicleDamage_FiresWhenHealthDecreases(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(sut);

        sut.AddGlobal("testVehicle", vehicle);

        sut.RunLuaScript("""
            addEventHandler("onVehicleDamage", testVehicle, function(loss)
                assertPrint("damage:" .. tostring(loss))
            end)
            """);

        vehicle.Health = 975;

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("damage:25");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnTrailerAttach_FiresWhenTrailerAttachedToTruck(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var truck = new Vehicle(411, Vector3.Zero).AssociateWith(sut);
        var trailer = new Vehicle(411, new Vector3(5, 0, 0)).AssociateWith(sut);

        sut.AddGlobal("testTrailer", trailer);

        sut.RunLuaScript("""
            addEventHandler("onTrailerAttach", testTrailer, function(vehicle)
                assertPrint("attach")
            end)
            """);

        truck.AttachTrailer(trailer);

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("attach");
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void OnTrailerDetach_FiresWhenTrailerDetachedFromTruck(
        AssertDataProvider assertDataProvider,
        IMtaServer sut)
    {
        var truck = new Vehicle(411, Vector3.Zero).AssociateWith(sut);
        var trailer = new Vehicle(411, new Vector3(5, 0, 0)).AssociateWith(sut);
        truck.AttachTrailer(trailer);

        sut.AddGlobal("testTrailer", trailer);

        sut.RunLuaScript("""
            addEventHandler("onTrailerDetach", testTrailer, function(vehicle)
                assertPrint("detach")
            end)
            """);

        truck.DetachTrailer();

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("detach");
    }
}
