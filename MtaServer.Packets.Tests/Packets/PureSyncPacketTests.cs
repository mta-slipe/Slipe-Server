using FluentAssertions;
using MtaServer.Packets.Definitions.Sync;
using System;
using System.Numerics;
using Xunit;

namespace MtaServer.Packets.Tests.Packets
{
    public class PureSyncPacketTests
    {
        private readonly byte[] testPacket = new byte[]
        {
            0, 0, 0, 0, 0, 96, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 3, 32, 5, 125, 221, 62, 143, 24, 0, 0,
        };
        private readonly byte[] testPacketTwo = new byte[]
        {
            0, 0, 0, 0, 2, 44, 47, 96, 78, 126, 175, 224, 0, 11, 115, 243, 52, 22, 64, 14, 203, 190, 124, 53, 131, 128, 0
        };
        private readonly byte[] testPacketThree = new byte[]
        {
            0, 8, 0, 129, 2, 96, 212, 80, 55, 123, 207, 216, 15, 236, 163, 253, 113, 173, 12, 201, 89, 241, 144, 53, 100, 0, 82, 238, 71, 225, 75, 0, 0
        };

        [Fact]
        public void ReadPacket_ReadsValuesProperly()
        {
            var packet = new PlayerPureSyncPacket();

            packet.Read(this.testPacket);

            packet.SyncFlags.IsInWater.Should().Be(false);
            packet.SyncFlags.IsOnGround.Should().Be(false);
            packet.SyncFlags.HasJetpack.Should().Be(false);
            packet.SyncFlags.IsDucked.Should().Be(false);
            packet.SyncFlags.WearsGoggles.Should().Be(false);
            packet.SyncFlags.HasContact.Should().Be(false);
            packet.SyncFlags.IsChoking.Should().Be(false);
            packet.SyncFlags.AkimboTargetUp.Should().Be(false);
            packet.SyncFlags.IsOnFire.Should().Be(false);
            packet.SyncFlags.HasAWeapon.Should().Be(true);
            packet.SyncFlags.IsSyncingVelocity.Should().Be(true);
            packet.SyncFlags.IsStealthAiming.Should().Be(false);

            packet.Position.Should().Be(Vector3.Zero);
            packet.Velocity.Should().Be(Vector3.Zero);

            packet.Health.Should().Be(100);
            packet.Armor.Should().Be(0);

            //packet.CameraRotation.Should().Be(0);
            packet.CameraOrientation.BasePosition.Should().Be(packet.Position);
            //packet.CameraOrientation.CameraPosition.Should().Be(Vector3.Zero);
            //packet.CameraOrientation.CameraForward.Should().Be(Vector3.Zero);
        }

        [Fact]
        public void ReadPacketTwo_ReadsValuesProperly()
        {
            var packet = new PlayerPureSyncPacket();

            packet.Read(this.testPacketTwo);

            packet.SyncFlags.IsInWater.Should().Be(false);
            packet.SyncFlags.IsOnGround.Should().Be(true);
            packet.SyncFlags.HasJetpack.Should().Be(false);
            packet.SyncFlags.IsDucked.Should().Be(false);
            packet.SyncFlags.WearsGoggles.Should().Be(false);
            packet.SyncFlags.HasContact.Should().Be(false);
            packet.SyncFlags.IsChoking.Should().Be(false);
            packet.SyncFlags.AkimboTargetUp.Should().Be(false);
            packet.SyncFlags.IsOnFire.Should().Be(false);
            packet.SyncFlags.HasAWeapon.Should().Be(true);
            packet.SyncFlags.IsSyncingVelocity.Should().Be(false);
            packet.SyncFlags.IsStealthAiming.Should().Be(false);

            packet.Position.Should().NotBe(Vector3.Zero);
            packet.Velocity.Should().Be(Vector3.Zero);

            packet.Health.Should().Be(100);
            packet.Armor.Should().Be(0);

            //packet.CameraRotation.Should().Be(0);
            packet.CameraOrientation.BasePosition.Should().Be(packet.Position);
            //packet.CameraOrientation.CameraPosition.Should().Be(Vector3.Zero);
            //packet.CameraOrientation.CameraForward.Should().Be(Vector3.Zero);
        }

        [Fact]
        public void ReadPacketThree_ReadsValuesProperly()
        {
            var packet = new PlayerPureSyncPacket();

            packet.Read(this.testPacketThree);

            packet.SyncFlags.IsInWater.Should().Be(false);
            packet.SyncFlags.IsOnGround.Should().Be(true);
            packet.SyncFlags.HasJetpack.Should().Be(false);
            packet.SyncFlags.IsDucked.Should().Be(false);
            packet.SyncFlags.WearsGoggles.Should().Be(false);
            packet.SyncFlags.HasContact.Should().Be(false);
            packet.SyncFlags.IsChoking.Should().Be(false);
            packet.SyncFlags.AkimboTargetUp.Should().Be(false);
            packet.SyncFlags.IsOnFire.Should().Be(false);
            packet.SyncFlags.HasAWeapon.Should().Be(true);
            packet.SyncFlags.IsSyncingVelocity.Should().Be(true);
            packet.SyncFlags.IsStealthAiming.Should().Be(false);
        }

    }
}
