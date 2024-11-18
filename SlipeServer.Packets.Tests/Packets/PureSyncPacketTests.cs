using FluentAssertions;
using SlipeServer.Packets.Definitions.Sync;
using System.Numerics;
using Xunit;

namespace SlipeServer.Packets.Tests.Packets;

public class PureSyncPacketTests
{
    private readonly byte[] testPacket =
    [
            2, 0, 0, 0, 0, 139, 173, 0, 3, 173, 20, 0, 2, 1, 29, 2, 1, 140, 200, 3, 33, 87, 29, 66, 172, 121, 93, 192, 80, 201, 23, 248,
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
