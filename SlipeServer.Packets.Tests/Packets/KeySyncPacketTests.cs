using FluentAssertions;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using System;
using System.Numerics;
using Xunit;

namespace SlipeServer.Packets.Tests.Packets
{
    public class KeySyncPacketTests
    {
        private readonly byte[] testPacket = new byte[]
        {
            16,0,0,37,153,146,98,0,0
        };

        [Fact]
        public void ReadPacket_ReadsValuesProperly()
        {
            var packet = new KeySyncPacket();

            packet.Read(this.testPacket);

            packet.SmallKeySyncStructure.ButtonCircle.Should().BeTrue();
            packet.HasWeapon.Should().Be(true);
            packet.AimOrigin.Should().Be(Vector3.Zero);
            packet.AimDirection.Should().Be(Vector3.Zero);
            packet.VehicleAimDirection.Should().Be(VehicleAimDirection.Forwards);
        }
    }
}
