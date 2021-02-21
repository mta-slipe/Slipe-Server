using FluentAssertions;
using SlipeServer.Packets.Definitions.Vehicles;
using System;
using System.Numerics;
using Xunit;

namespace SlipeServer.Packets.Tests.Packets
{
    public class VehiclePureSyncPacketTests
    {
        private readonly byte[] carTestPacket = new byte[]
        {
            2, 0, 0, 0, 90, 2, 0, 0, 0, 216, 255, 3, 20, 0, 16, 144, 57, 64, 102, 105, 44, 170, 195, 51, 252, 0, 0, 0, 0, 208, 112, 200, 0, 193, 0, 0,
        };
        private readonly byte[] hydraTestPacket = new byte[]
        {
            2, 0, 0, 0, 8, 2, 0, 0, 117, 111, 4, 93, 29, 1, 221, 83, 140, 67, 182, 153, 66, 16, 56, 92, 7, 71, 192, 0, 21, 114, 101, 81, 216, 39, 199, 124, 194, 127, 2, 213, 166, 128, 119, 12, 126, 50, 223, 84, 92, 50, 0, 0, 97, 16, 38, 0, 0,
        };
        private readonly byte[] forkliftTestPacket = new byte[]
        {
            2, 0, 0, 0, 18, 2, 0, 0, 165, 45, 0, 85, 44, 0, 132, 101, 56, 64, 96, 125, 43, 163, 129, 75, 255, 255, 255, 229, 56, 208, 112, 200, 0, 193, 4, 128, 24,
        };
        private readonly byte[] firetruckTestPacket = new byte[]
        {
            2, 0, 0, 0, 151, 1, 0, 0, 0, 120, 0, 241, 19, 0, 250, 64, 85, 64, 178, 120, 58, 155, 1, 8, 0, 0, 0, 0, 0, 208, 112, 200, 0, 193, 3, 38, 111, 152, 17, 1, 0,
        };

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

            packet.AdjustableProperty.HasValue.Should().BeFalse();
            packet.TurretRotation.HasValue.Should().BeFalse();
        }

        [Fact]
        public void ReadHydraPacket_ReadsValuesProperly()
        {
            var packet = new VehiclePureSyncPacket();

            packet.Read(this.hydraTestPacket);

            packet.RemoteModel.Should().Be(520);
            packet.Position.Should().Equals(new Vector3(10, 5, 3));
            packet.Seat.Should().Be(0);
            packet.Health.Should().Be(936.5f);
            packet.PlayerHealth.Should().Be(50);
            packet.PlayerArmor.Should().Be(0);

            packet.VehiclePureSyncFlags.IsLandingGearDown.Should().BeFalse();

            packet.AdjustableProperty.HasValue.Should().BeTrue();
            packet.TurretRotation.HasValue.Should().BeFalse();
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

            packet.AdjustableProperty.HasValue.Should().BeTrue();
            packet.TurretRotation.HasValue.Should().BeFalse();
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

            packet.AdjustableProperty.HasValue.Should().BeFalse();
            packet.TurretRotation.HasValue.Should().BeTrue();
        }
    }
}
