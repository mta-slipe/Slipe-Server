using FluentAssertions;
using SlipeServer.DropInReplacement.MixedResources.Behaviour;
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
    private static string ResourceDirectory => Path.Combine(AppContext.BaseDirectory, "Resources");

    private static (DropInReplacementTestingServer server, LightTestPlayer player) CreateStartedServer()
    {
        var server = new DropInReplacementTestingServer(ResourceDirectory);
        var service = server.GetRequiredService<IDropInReplacementResourceService>();
        service.StartResource("freeroam");
        var player = server.JoinFakePlayer();
        return (server, player);
    }

    private static void TriggerRpc(DropInReplacementTestingServer server, LightTestPlayer player, string fnName, params object[] args)
    {
        var eventRuntime = server.GetRequiredService<IScriptEventRuntime>();
        var resourceService = server.GetRequiredService<DropInReplacementResourceService>();
        var resource = resourceService.StartedResources.First(r => r.Name.Equals("freeroam", StringComparison.OrdinalIgnoreCase));

        var allArgs = new object[] { fnName }.Concat(args).ToArray();
        eventRuntime.TriggerCustomEventFromClient("onServerCall", resource.Root, player, allArgs);
    }

    [Fact]
    public void SetMySkin_WithValidSkinId_ChangesPlayerModel()
    {
        var (server, player) = CreateStartedServer();
        player.Position = new Vector3(100, 200, 10);

        TriggerRpc(server, player, "setMySkin", 25);

        player.Model.Should().Be(25);
    }

    [Fact]
    public void GiveMeWeapon_WithValidWeapon_GivesWeaponToPlayer()
    {
        var (server, player) = CreateStartedServer();

        TriggerRpc(server, player, "giveMeWeapon", 31, 100); // M4, 100 ammo

        player.Weapons.Should().NotBeEmpty();
    }

    [Fact]
    public void WarpMeIntoVehicle_WithValidVehicle_WarpsPlayerIntoVehicle()
    {
        var (server, player) = CreateStartedServer();
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(server);

        TriggerRpc(server, player, "warpMeIntoVehicle", vehicle);

        player.Vehicle.Should().Be(vehicle);
    }

    [Fact]
    public void SetElementAlpha_WithValidAlpha_ChangesPlayerAlpha()
    {
        var (server, player) = CreateStartedServer();

        TriggerRpc(server, player, "setElementAlpha", player, 128);

        player.Alpha.Should().Be(128);
    }

    [Fact]
    public void SetElementPosition_WithValidCoords_ChangesPlayerPosition()
    {
        var (server, player) = CreateStartedServer();

        TriggerRpc(server, player, "setElementPosition", player, 100f, 200f, 10f);

        player.Position.Should().Be(new Vector3(100f, 200f, 10f));
    }

    [Fact]
    public void SetElementInterior_WithValidInterior_ChangesPlayerInterior()
    {
        var (server, player) = CreateStartedServer();

        TriggerRpc(server, player, "setElementInterior", player, 1);

        player.Interior.Should().Be(1);
    }

    [Fact]
    public void SetPedGravity_WithValidGravity_ChangesPlayerGravity()
    {
        var (server, player) = CreateStartedServer();

        TriggerRpc(server, player, "setPedGravity", player, 0.05f);

        player.Gravity.Should().BeApproximately(0.05f, 0.001f);
    }

    [Fact]
    public void SetPedFightingStyle_WithValidStyle_ChangesFightingStyle()
    {
        var (server, player) = CreateStartedServer();

        TriggerRpc(server, player, "setPedFightingStyle", player, 5); // Boxing

        player.FightingStyle.Should().Be(FightingStyle.Boxing);
    }

    [Fact]
    public void FixVehicle_WhenPlayerInVehicle_FixesVehicle()
    {
        var (server, player) = CreateStartedServer();
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(server);
        player.WarpIntoVehicle(vehicle);
        vehicle.BlowUp();

        TriggerRpc(server, player, "fixVehicle", vehicle);

        vehicle.Health.Should().Be(1000);
        vehicle.BlownState.Should().Be(VehicleBlownState.Intact);
    }

    [Fact]
    public void RemovePedFromVehicle_WhenPlayerInVehicle_RemovesPlayerFromVehicle()
    {
        var (server, player) = CreateStartedServer();
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(server);
        player.WarpIntoVehicle(vehicle);

        TriggerRpc(server, player, "removePedFromVehicle", player);

        player.Vehicle.Should().BeNull();
    }

    [Fact]
    public void SetElementPosition_ForOccupiedVehicle_ChangesVehiclePosition()
    {
        var (server, player) = CreateStartedServer();
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(server);
        player.WarpIntoVehicle(vehicle);

        TriggerRpc(server, player, "setElementPosition", vehicle, 50f, 60f, 5f);

        vehicle.Position.Should().Be(new Vector3(50f, 60f, 5f));
    }

    [Fact]
    public void GiveMeVehicles_WithValidVehicleId_CreatesVehicleNearPlayer()
    {
        var (server, player) = CreateStartedServer();
        var elementCollection = server.GetRequiredService<IElementCollection>();
        var vehiclesBefore = elementCollection.GetByType<Vehicle>().ToList();

        TriggerRpc(server, player, "giveMeVehicles", 411); // Infernus

        var vehiclesAfter = elementCollection.GetByType<Vehicle>().ToList();
        vehiclesAfter.Should().HaveCount(vehiclesBefore.Count + 1);
        vehiclesAfter.Except(vehiclesBefore).Single().Model.Should().Be(411);
    }

    [Fact]
    public void SetElementInterior_ForOccupiedVehicle_ChangesVehicleInterior()
    {
        var (server, player) = CreateStartedServer();
        var vehicle = new Vehicle(411, Vector3.Zero).AssociateWith(server);
        player.WarpIntoVehicle(vehicle);

        TriggerRpc(server, player, "setElementInterior", vehicle, 2);

        vehicle.Interior.Should().Be(2);
    }
}
