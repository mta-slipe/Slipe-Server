using FluentAssertions;
using SlipeServer.Packets.Definitions.Vehicles;
using System.Numerics;
using Xunit;

namespace SlipeServer.Packets.Tests.Packets;

public class VehicleDamageSyncPacketTests
{
    private readonly byte[] testPacket = new byte[]
    {
            44, 0, 80, 0, 20, 0
    };
    private readonly byte[] testPacket2 = new byte[]
    {
            44, 0, 40, 0, 10, 0
    };

    [Fact]
    public void ReadPacket_ReadsValuesProperly()
    {
        var packet = new VehicleDamageSyncPacket();

        packet.Read(this.testPacket);

        packet.VehicleId.Should().Be(44);
        packet.DoorStates.Should().BeEquivalentTo(new byte?[] { 2, null, null, null, null, null });
        packet.WheelStates.Should().BeEquivalentTo(new byte?[] { null, null, null, null });
        packet.PanelStates.Should().BeEquivalentTo(new byte?[] { null, null, null, null, null, 1, null });
        packet.LightStates.Should().BeEquivalentTo(new byte?[] { null, null, null, null });
    }

    [Fact]
    public void ReadPacket2_ReadsValuesProperly()
    {
        var packet = new VehicleDamageSyncPacket();

        packet.Read(this.testPacket2);

        packet.VehicleId.Should().Be(44);
        packet.DoorStates.Should().BeEquivalentTo(new byte?[] { null, 2, null, null, null, null });
        packet.WheelStates.Should().BeEquivalentTo(new byte?[] { null, null, null, null });
        packet.PanelStates.Should().BeEquivalentTo(new byte?[] { null, null, null, null, null, null, 1 });
        packet.LightStates.Should().BeEquivalentTo(new byte?[] { null, null, null, null });
    }
}
