using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SlipeServer.DropInReplacement.PacketHandlers;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Scripting;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Scenarios;

/// <summary>
/// Verifies that calling cancelEvent() inside onVehicleStartEnter / onVehicleStartExit handlers
/// prevents the server from confirming the entry or exit, leaving the player's vehicle state unchanged.
/// Vehicle must be AssociateWith(sut) BEFORE RunLuaScript so addEventHandler iterates GetAll()
/// and attaches the proxy to the vehicle's TriggerPedStartedEntering/Exiting events immediately.
/// </summary>
public class VehicleInOutCancellationTests
{
    private static ScriptingVehicleInOutPacketHandler CreateHandler(IMtaServer sut) =>
        new(
            sut.GetRequiredService<IElementCollection>(),
            sut,
            sut.GetRequiredService<ILogger>(),
            sut.GetRequiredService<IScriptEventRuntime>()
        );

    private static VehicleInOutPacket RequestInPacket(LightTestPlayer player, Vehicle vehicle, byte seat = 0) =>
        new()
        {
            PedId = player.Id,
            VehicleId = vehicle.Id,
            ActionId = VehicleInOutAction.RequestIn,
            Seat = seat,
            Door = 0,
        };

    private static VehicleInOutPacket RequestOutPacket(LightTestPlayer player, Vehicle vehicle) =>
        new()
        {
            PedId = player.Id,
            VehicleId = vehicle.Id,
            ActionId = VehicleInOutAction.RequestOut,
            Door = 0,
        };

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void CancelEvent_InOnVehicleStartEnter_OnVehicle_PreventsEntry(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(sut);
        player.Position = Vector3.Zero;

        sut.AddGlobal("testVehicle", vehicle);
        sut.RunLuaScript("""
            addEventHandler("onVehicleStartEnter", testVehicle, function(enteringPed, seat, jacked, door)
                assertPrint("event fired")
                cancelEvent()
            end)
            """);

        var handler = CreateHandler(sut);
        handler.HandlePacket(player.Client, RequestInPacket(player, vehicle));

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("event fired");
        player.VehicleAction.Should().Be(VehicleAction.None);
        player.EnteringVehicle.Should().BeNull();
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void CancelEvent_InOnVehicleStartEnter_OnRoot_PreventsEntry(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(sut);
        player.Position = Vector3.Zero;

        sut.RunLuaScript("""
            addEventHandler("onVehicleStartEnter", root, function(enteringPed, seat, jacked, door)
                assertPrint("event fired")
                cancelEvent()
            end)
            """);

        var handler = CreateHandler(sut);
        handler.HandlePacket(player.Client, RequestInPacket(player, vehicle));

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("event fired");
        player.VehicleAction.Should().Be(VehicleAction.None);
        player.EnteringVehicle.Should().BeNull();
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void NoCancelEvent_InOnVehicleStartEnter_AllowsEntry(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(sut);
        player.Position = Vector3.Zero;

        sut.AddGlobal("testVehicle", vehicle);
        sut.RunLuaScript("""
            addEventHandler("onVehicleStartEnter", testVehicle, function(enteringPed, seat, jacked, door)
                assertPrint("event fired")
            end)
            """);

        var handler = CreateHandler(sut);
        handler.HandlePacket(player.Client, RequestInPacket(player, vehicle));

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("event fired");
        player.VehicleAction.Should().Be(VehicleAction.Entering);
        player.EnteringVehicle.Should().Be(vehicle);
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void CancelEvent_InOnVehicleStartEnter_PassengerSeat_PreventsEntry(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(sut);
        player.Position = Vector3.Zero;

        sut.AddGlobal("testVehicle", vehicle);
        sut.RunLuaScript("""
            addEventHandler("onVehicleStartEnter", testVehicle, function(enteringPed, seat, jacked, door)
                assertPrint("event fired:" .. tostring(seat))
                cancelEvent()
            end)
            """);

        var handler = CreateHandler(sut);
        handler.HandlePacket(player.Client, RequestInPacket(player, vehicle, seat: 1));

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("event fired:1");
        player.VehicleAction.Should().Be(VehicleAction.None);
        player.EnteringVehicle.Should().BeNull();
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void CancelEvent_InOnVehicleStartExit_OnVehicle_PreventsExit(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(sut);
        player.WarpIntoVehicle(vehicle);

        sut.AddGlobal("testVehicle", vehicle);
        sut.RunLuaScript("""
            addEventHandler("onVehicleStartExit", testVehicle, function(exitingPed, seat, jacker, door)
                assertPrint("event fired")
                cancelEvent()
            end)
            """);

        var handler = CreateHandler(sut);
        handler.HandlePacket(player.Client, RequestOutPacket(player, vehicle));

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("event fired");
        player.VehicleAction.Should().Be(VehicleAction.None);
        player.Vehicle.Should().Be(vehicle);
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void NoCancelEvent_InOnVehicleStartExit_AllowsExit(
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        IMtaServer sut)
    {
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(sut);
        player.WarpIntoVehicle(vehicle);

        sut.AddGlobal("testVehicle", vehicle);
        sut.RunLuaScript("""
            addEventHandler("onVehicleStartExit", testVehicle, function(exitingPed, seat, jacker, door)
                assertPrint("event fired")
            end)
            """);

        var handler = CreateHandler(sut);
        handler.HandlePacket(player.Client, RequestOutPacket(player, vehicle));

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("event fired");
        player.VehicleAction.Should().Be(VehicleAction.Exiting);
    }
}
