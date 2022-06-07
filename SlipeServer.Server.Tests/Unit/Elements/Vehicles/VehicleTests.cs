using FluentAssertions;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
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

        vehicle.Position.Should().Be(Vector3.Zero);
        vehicle.Rotation.Should().Be(Vector3.Zero);
        vehicle.IsLandingGearDown.Should().Be(true);
        vehicle.Health.Should().Be(1000);
        vehicle.BlownState.Should().Be(VehicleBlownState.Intact);
        vehicle.Velocity.Should().Be(Vector3.Zero);
    }
}
