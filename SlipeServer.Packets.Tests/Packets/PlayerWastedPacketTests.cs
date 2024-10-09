using FluentAssertions;
using SlipeServer.Packets.Definitions.Player;
using Xunit;

namespace SlipeServer.Packets.Tests.Packets;

public class PlayerWastedPacketTests
{
    private readonly byte[] testPacket =
    [
            240, 232, 192, 224, 6, 163, 149, 228, 3, 40, 116, 12, 119, 72, 123, 7, 0,
    ];

    [Fact]
    public void ReadPacket_ReadsValuesProperly()
    {
        var packet = new PlayerWastedPacket();

        packet.Read(this.testPacket);

        packet.WeaponType.Should().Be(53); // Drowning
        packet.Stealth.Should().Be(false);
        packet.Ammo.Should().Be(0);
    }
}
