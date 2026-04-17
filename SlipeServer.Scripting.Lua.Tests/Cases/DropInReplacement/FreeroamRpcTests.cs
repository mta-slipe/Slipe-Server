using FluentAssertions;
using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.Scripting;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;

namespace SlipeServer.Scripting.Lua.Tests.Cases.DropInReplacement;

public class FreeroamRpcTests
{
    private static void TriggerRpc(
        IScriptEventRuntime eventRuntime,
        DropInReplacementResourceService resourceService,
        Player player,
        string fnName,
        params object[] args)
    {
        var resource = resourceService.StartedResources.First(r => r.Name.Equals("freeroam", StringComparison.OrdinalIgnoreCase));
        var allArgs = new object[] { fnName }.Concat(args).ToArray();
        eventRuntime.TriggerCustomEventFromClient("onServerCall", resource.Root, player, allArgs);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void SetMySkin_WithValidSkinId_ChangesPlayerModel(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        DropInReplacementResourceService resourceService)
    {
        service.StartResource("freeroam");
        var player = server.JoinFakePlayer();
        player.Position = new Vector3(100, 200, 10);

        TriggerRpc(eventRuntime, resourceService, player, "setMySkin", 25);

        player.Model.Should().Be(25);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void GiveMeWeapon_WithValidWeapon_GivesWeaponToPlayer(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        DropInReplacementResourceService resourceService)
    {
        service.StartResource("freeroam");
        var player = server.JoinFakePlayer();

        TriggerRpc(eventRuntime, resourceService, player, "giveMeWeapon", 31, 100);

        player.Weapons.Should().NotBeEmpty();
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void WarpMeIntoVehicle_WithValidVehicle_WarpsPlayerIntoVehicle(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        DropInReplacementResourceService resourceService)
    {
        service.StartResource("freeroam");
        var player = server.JoinFakePlayer();
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(server);

        TriggerRpc(eventRuntime, resourceService, player, "warpMeIntoVehicle", vehicle);

        player.Vehicle.Should().Be(vehicle);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void SetElementAlpha_WithValidAlpha_ChangesPlayerAlpha(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        DropInReplacementResourceService resourceService)
    {
        service.StartResource("freeroam");
        var player = server.JoinFakePlayer();

        TriggerRpc(eventRuntime, resourceService, player, "setElementAlpha", player, 128);

        player.Alpha.Should().Be(128);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void SetElementPosition_WithValidCoords_ChangesPlayerPosition(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        DropInReplacementResourceService resourceService)
    {
        service.StartResource("freeroam");
        var player = server.JoinFakePlayer();

        TriggerRpc(eventRuntime, resourceService, player, "setElementPosition", player, 100f, 200f, 10f);

        player.Position.Should().Be(new Vector3(100f, 200f, 10f));
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void SetElementInterior_WithValidInterior_ChangesPlayerInterior(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        DropInReplacementResourceService resourceService)
    {
        service.StartResource("freeroam");
        var player = server.JoinFakePlayer();

        TriggerRpc(eventRuntime, resourceService, player, "setElementInterior", player, 1);

        player.Interior.Should().Be(1);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void SetPedGravity_WithValidGravity_ChangesPlayerGravity(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        DropInReplacementResourceService resourceService)
    {
        service.StartResource("freeroam");
        var player = server.JoinFakePlayer();

        TriggerRpc(eventRuntime, resourceService, player, "setPedGravity", player, 0.05f);

        player.Gravity.Should().BeApproximately(0.05f, 0.001f);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void SetPedFightingStyle_WithValidStyle_ChangesFightingStyle(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        DropInReplacementResourceService resourceService)
    {
        service.StartResource("freeroam");
        var player = server.JoinFakePlayer();

        TriggerRpc(eventRuntime, resourceService, player, "setPedFightingStyle", player, 5);

        player.FightingStyle.Should().Be(FightingStyle.Boxing);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void FixVehicle_WhenPlayerInVehicle_FixesVehicle(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        DropInReplacementResourceService resourceService)
    {
        service.StartResource("freeroam");
        var player = server.JoinFakePlayer();
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(server);
        player.WarpIntoVehicle(vehicle);
        vehicle.BlowUp();

        TriggerRpc(eventRuntime, resourceService, player, "fixVehicle", vehicle);

        vehicle.Health.Should().Be(1000);
        vehicle.BlownState.Should().Be(VehicleBlownState.Intact);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void RemovePedFromVehicle_WhenPlayerInVehicle_RemovesPlayerFromVehicle(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        DropInReplacementResourceService resourceService)
    {
        service.StartResource("freeroam");
        var player = server.JoinFakePlayer();
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(server);
        player.WarpIntoVehicle(vehicle);

        TriggerRpc(eventRuntime, resourceService, player, "removePedFromVehicle", player);

        player.Vehicle.Should().BeNull();
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void SetElementPosition_ForOccupiedVehicle_ChangesVehiclePosition(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        DropInReplacementResourceService resourceService)
    {
        service.StartResource("freeroam");
        var player = server.JoinFakePlayer();
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(server);
        player.WarpIntoVehicle(vehicle);

        TriggerRpc(eventRuntime, resourceService, player, "setElementPosition", vehicle, 50f, 60f, 5f);

        vehicle.Position.Should().Be(new Vector3(50f, 60f, 5f));
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void GiveMeVehicles_WithValidVehicleId_CreatesVehicleNearPlayer(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        DropInReplacementResourceService resourceService,
        IElementCollection elementCollection)
    {
        service.StartResource("freeroam");
        var player = server.JoinFakePlayer();
        var vehiclesBefore = elementCollection.GetByType<Vehicle>().ToList();

        TriggerRpc(eventRuntime, resourceService, player, "giveMeVehicles", 411);

        var vehiclesAfter = elementCollection.GetByType<Vehicle>().ToList();
        vehiclesAfter.Should().HaveCount(vehiclesBefore.Count + 1);
        vehiclesAfter.Except(vehiclesBefore).Single().Model.Should().Be(411);
    }

    [Theory]
    [DropInReplacementAutoDomainData]
    public void SetElementInterior_ForOccupiedVehicle_ChangesVehicleInterior(
        DropInReplacementTestingServer server,
        IDropInReplacementResourceService service,
        IScriptEventRuntime eventRuntime,
        DropInReplacementResourceService resourceService)
    {
        service.StartResource("freeroam");
        var player = server.JoinFakePlayer();
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(server);
        player.WarpIntoVehicle(vehicle);

        TriggerRpc(eventRuntime, resourceService, player, "setElementInterior", vehicle, 2);

        vehicle.Interior.Should().Be(2);
    }
}
