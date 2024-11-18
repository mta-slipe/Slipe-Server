using FluentAssertions;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Structs;
using System.Numerics;
using Xunit;

namespace SlipeServer.Packets.Tests.Packets;

public class ProjectilePacketTests
{
    private readonly byte[] testPacket =
    [
            23, 132, 0, 5, 112, 255, 129, 105, 193, 160, 38, 178, 2, 230, 230, 102, 31, 68, 43, 137, 90, 127, 79, 218, 11, 224, 0, 0, 3, 42, 252, 106, 35, 240
    ];

    [Fact]
    public void ReadPacket_ReadsValuesProperly()
    {
        var packet = new ProjectileSyncPacket();

        packet.Read(this.testPacket);

        packet.HasTarget.Should().Be(0);
        packet.Force.Should().Be(0);
        packet.Model.Should().Be(345);
        packet.WeaponType.Should().Be(19);
        packet.MoveSpeed.Should().Be(new Vector3(-0.3635011f, -0.112092786f, 0.12370186f));
        packet.VecOrigin.Should().Be(new Vector3(2.0458984f, -7.7402344f, 4.119508f));
        packet.Rotation.Should().Be(new Vector3(-0.3144375f, 7.450581E-09f, 1.2716883f));
        packet.SourceElement.Should().Be(ElementId.Zero);
        packet.OriginId.Should().Be(ElementId.Zero);
        packet.TargetId.Should().Be(ElementId.Zero);
        packet.VecTarget.Should().Be(Vector3.Zero);
    }
}
