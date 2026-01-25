using FluentAssertions;
using SlipeServer.Packets.Definitions.Sync;
using System.Numerics;
using Xunit;

namespace SlipeServer.Packets.Tests.Packets;

public class PureSyncPacketTests
{
    private readonly byte[] testPacket =
    [
        3, 0, 0, 0, 0, 136, 227, 213, 255, 10, 144, 0, 1, 0, 142, 128, 43, 76, 100, 1, 60, 171, 78, 96, 212, 0, 0
    ];

    [Fact]
    public void ReadPacket_ReadsValuesProperly()
    {
        var packet = new PlayerPureSyncPacket();

        packet.Read(this.testPacket);

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

        packet.Health.Should().Be(50);
        packet.Armor.Should().Be(0);

        packet.CameraOrientation.BasePosition.Should().Be(packet.Position);
    }
}
