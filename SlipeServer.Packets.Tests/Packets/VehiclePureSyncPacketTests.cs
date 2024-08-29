using FluentAssertions;
using SlipeServer.Packets.Definitions.Vehicles;
using System.Numerics;
using Xunit;

namespace SlipeServer.Packets.Tests.Packets;

public class VehiclePureSyncPacketTests
{
    private readonly byte[] carTestPacket =
    [
            2, 0, 0, 0, 22, 128, 128, 0, 15, 55, 127, 203, 199, 192, 31, 4, 206, 208, 41, 28, 141, 104, 192, 215, 255, 1, 0, 59, 238, 52, 28, 50, 0, 48, 64, 0,
    ];
    private readonly byte[] hydraTestPacket =
    [
            2, 0, 0, 0, 2, 0, 128, 0, 0, 202, 0, 40, 132, 192, 16, 220, 29, 80, 54, 93, 15, 200, 144, 105, 1, 0, 0, 246, 255, 52, 28, 50, 0, 48, 96, 240, 20, 0, 0,
    ];
    private readonly byte[] forkliftTestPacket =
    [
            2, 0, 0, 0, 4, 128, 128, 0, 0, 20, 0, 3, 133, 0, 26, 89, 78, 16, 62, 94, 12, 20, 208, 82, 255, 255, 255, 0, 0, 52, 28, 50, 0, 48, 65, 208, 4,
    ];
    private readonly byte[] firetruckTestPacket =
    [
            2, 0, 0, 0, 37, 192, 64, 0, 10, 30, 0, 3, 133, 0, 6, 94, 149, 80, 44, 158, 142, 166, 208, 64, 0, 1, 0, 54, 255, 52, 28, 50, 0, 48, 64, 161, 156, 28, 10, 0,
    ];

    [Fact]
    public void ReadCarPacket_ReadsValuesProperly()
    {
        var packet = new VehiclePureSyncPacket();

        packet.Read(this.carTestPacket);

        packet.RemoteModel.Should().Be(602);
        packet.Position.Should().Equals(new Vector3(-10, 5, 3));
        packet.Seat.Should().Be(0);
        packet.Health.Should().Be(1000);
        packet.PlayerHealth.Should().Be(50);
        packet.PlayerArmor.Should().Be(0);

        packet.AdjustableProperty.Should().Be(0);
        packet.TurretRotation.Should().Be(Vector2.Zero);
    }

    [Fact]
    public void ReadHydraPacket_ReadsValuesProperly()
    {
        var packet = new VehiclePureSyncPacket();

        packet.Read(this.hydraTestPacket);

        packet.RemoteModel.Should().Be(520);
        packet.Position.Should().Equals(new Vector3(10, 5, 3));
        packet.Seat.Should().Be(0);
        packet.Health.Should().Be(1000);
        packet.PlayerHealth.Should().Be(50);
        packet.PlayerArmor.Should().Be(0);

        packet.VehiclePureSyncFlags.IsLandingGearDown.Should().BeTrue();

        packet.AdjustableProperty.Should().NotBe(0);
        packet.TurretRotation.Should().Be(Vector2.Zero);
    }

    [Fact]
    public void ReadForkliftPacket_ReadsValuesProperly()
    {
        var packet = new VehiclePureSyncPacket();

        packet.Read(this.forkliftTestPacket);

        packet.RemoteModel.Should().Be(530);
        packet.Position.Should().Equals(new Vector3(20, 5, 3));
        packet.Seat.Should().Be(0);
        packet.Health.Should().Be(1000);
        packet.PlayerHealth.Should().Be(50);
        packet.PlayerArmor.Should().Be(0);

        packet.AdjustableProperty.Should().NotBe(0);
        packet.TurretRotation.Should().Be(Vector2.Zero);
    }

    [Fact]
    public void ReadFireTruckPacket_ReadsValuesProperly()
    {
        var packet = new VehiclePureSyncPacket();

        packet.Read(this.firetruckTestPacket);

        packet.RemoteModel.Should().Be(407);
        packet.Position.Should().Equals(new Vector3(30, 5, 3));
        packet.Seat.Should().Be(0);
        packet.Health.Should().Be(1000);
        packet.PlayerHealth.Should().Be(50);
        packet.PlayerArmor.Should().Be(0);

        packet.AdjustableProperty.Should().Be(0);
        packet.TurretRotation.Should().NotBe(Vector2.Zero);
    }
}
