using FluentAssertions;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structures;
using Xunit;

namespace SlipeServer.Packets.Tests.Structs;

public class VehiclePureSyncFlagsTest
{
    [Fact]
    public void ReadVehiclePureSync_ReturnsProperValues()
    {
        var reader = new PacketReader([0b10101111, 0b11000000]);
        var structure = new VehiclePureSyncFlagsStructure();

        structure.Read(reader);

        structure.IsWearingGoggles.Should().Be(true);
        structure.IsDoingGangDriveby.Should().Be(true);
        structure.IsSirenOrAlarmActive.Should().Be(true);
        structure.IsSmokeTrailEnabled.Should().Be(true);
        structure.IsLandingGearDown.Should().Be(false);
        structure.IsOnGround.Should().Be(true);
        structure.IsInWater.Should().Be(false);
        structure.IsDerailed.Should().Be(true);
        structure.IsAircraft.Should().Be(false);
        structure.HasAWeapon.Should().Be(true);
        structure.IsHeliSearchLightVisible.Should().Be(true);
    }
}
