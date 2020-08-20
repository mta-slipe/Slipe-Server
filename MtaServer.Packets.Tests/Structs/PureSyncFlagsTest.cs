using FluentAssertions;
using MtaServer.Packets.Builder;
using MtaServer.Packets.Definitions.Sync;
using MtaServer.Packets.Reader;
using MtaServer.Packets.Structures;
using System;
using System.Numerics;
using Xunit;

namespace MtaServer.Packets.Tests.Structs
{
    public class PureSyncFlagsTest
    {
        [Fact]
        public void Read_ReturnsProperValues()
        {
            var reader = new PacketReader(new byte[] { 0b10101111, 0b11000000 });
            var structure = new PlayerPureSyncFlagsStructure();

            structure.Read(reader);

            structure.IsInWater.Should().Be(true);
            structure.IsOnGround.Should().Be(true);
            structure.HasJetpack.Should().Be(true);
            structure.IsDucked.Should().Be(true);
            structure.WearsGoggles.Should().Be(false);
            structure.HasContact.Should().Be(true);
            structure.IsChoking.Should().Be(false);
            structure.AkimboTargetUp.Should().Be(true);
            structure.IsOnFire.Should().Be(false);
            structure.HasAWeapon.Should().Be(false);
            structure.IsSyncingVelocity.Should().Be(true);
            structure.IsStealthAiming.Should().Be(true);
        }

        [Fact]
        public void ReadTwo_ReturnsProperValues()
        {
            var reader = new PacketReader(new byte[] { 0b00111000, 0b11100000 });
            var structure = new PlayerPureSyncFlagsStructure();

            structure.Read(reader);

            structure.IsInWater.Should().Be(false);
            structure.IsOnGround.Should().Be(false);
            structure.HasJetpack.Should().Be(false);
            structure.IsDucked.Should().Be(true);
            structure.WearsGoggles.Should().Be(true);
            structure.HasContact.Should().Be(true);
            structure.IsChoking.Should().Be(false);
            structure.AkimboTargetUp.Should().Be(false);
            structure.IsOnFire.Should().Be(false);
            structure.HasAWeapon.Should().Be(true);
            structure.IsSyncingVelocity.Should().Be(true);
            structure.IsStealthAiming.Should().Be(true);
        }         

        [Fact]
        public void Write_WritesProperValues()
        {
            var builder = new PacketBuilder();
            var structure = new PlayerPureSyncFlagsStructure()
            {
                IsInWater = true,
                IsOnGround = true,
                HasJetpack = true,
                IsDucked = true,
                WearsGoggles = false,
                HasContact = true,
                IsChoking = false,
                AkimboTargetUp = true,
                IsOnFire = false,
                HasAWeapon = false,
                IsSyncingVelocity = true,
                IsStealthAiming = true
            };

            structure.Write(builder);
            var data = builder.Build();

            data.Should().Equal(new byte[] { 0b10101111, 0b11000000 });
        }
    }
}
