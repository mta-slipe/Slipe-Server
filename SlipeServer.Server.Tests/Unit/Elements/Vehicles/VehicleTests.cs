using FluentAssertions;
using FluentAssertions.Execution;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Elements.Vehicles;

public class VehicleTests
{
    [Fact]
    public void VehicleRespawn_ShouldResetDefaults()
    {
        var vehicle = new Vehicle(VehicleModel.Alpha, Vector3.Zero);

        vehicle.Position += Vector3.UnitZ;
        vehicle.Rotation += new Vector3(90, 180, 270);
        vehicle.IsLandingGearDown = false;
        vehicle.Health = 125;
        vehicle.BlownState = VehicleBlownState.AwaitingExplosion;
        vehicle.Velocity = Vector3.UnitZ;

        vehicle.Respawn();

        using var _ = new AssertionScope();

        vehicle.Position.Should().Be(Vector3.Zero);
        vehicle.Rotation.Should().Be(Vector3.Zero);
        vehicle.IsLandingGearDown.Should().Be(true);
        vehicle.Health.Should().Be(1000);
        vehicle.BlownState.Should().Be(VehicleBlownState.Intact);
        vehicle.Velocity.Should().Be(Vector3.Zero);
    }

    [Fact]
    public void VehicleDamageEvents()
    {
        using var _ = new AssertionScope();

        var vehicle = new Vehicle(VehicleModel.Alpha, Vector3.Zero);
        using var monitor = vehicle.Monitor();

        vehicle.Fix();
        monitor.OccurredEvents.Select(x => x.EventName).Should().BeEquivalentTo(["Fixed"]); // Fixing already fixed, not damaged vehicle should raise only "fixed" event.

        vehicle.Health = 100;
        vehicle.SetDoorState(Packets.Enums.VehicleDoor.Hood, Packets.Enums.VehicleDoorState.Missing);
        vehicle.SetWheelState(Packets.Enums.VehicleWheel.FrontLeft, Packets.Enums.VehicleWheelState.FallenOff);
        vehicle.SetPanelState(Packets.Enums.VehiclePanel.FrontBumper, Packets.Enums.VehiclePanelState.Damaged3);

        monitor.Clear();
        vehicle.Fix();
        monitor.OccurredEvents.Select(x => x.EventName).Should().BeEquivalentTo(["HealthChanged", "DoorStateChanged", "WheelStateChanged", "PanelStateChanged", "Fixed"]);
    }

    [Fact]
    public void VehicleColors_UnusedColorsAreNull()
    {
        var vehicle = new Vehicle(VehicleModel.Alpha, Vector3.Zero);

        vehicle.Colors.AsArray().Should().Equal([Color.White, Color.White, null, null]);
    }

    [Fact]
    public void VehicleColors_DoNotRaiseColorChangedWhenColorIsUnchanged()
    {
        var vehicle = new Vehicle(VehicleModel.Alpha, Vector3.Zero);
        using var monitor = vehicle.Monitor();

        var primary = vehicle.Colors.Primary;
        vehicle.Colors.Primary = primary;
        vehicle.Colors.Color3 = null;

        monitor.OccurredEvents.Select(x => x.EventName).Should().NotContain("ColorChanged");
    }

    [Fact]
    public void VehicleColors_RaiseColorChangedWhenColorChanges()
    {
        var vehicle = new Vehicle(VehicleModel.Alpha, Vector3.Zero);
        using var monitor = vehicle.Monitor();

        vehicle.Colors.Primary = Color.Red;
        vehicle.Colors.Color3 = Color.Blue;

        monitor.OccurredEvents.Select(x => x.EventName).Should().Equal("ColorChanged", "ColorChanged");
    }
}
