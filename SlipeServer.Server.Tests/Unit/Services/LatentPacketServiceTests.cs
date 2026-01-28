using FluentAssertions;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Services;
using SlipeServer.Server.Tests.Tools;
using System;
using System.Linq;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Services;

public class LatentPacketServiceTests
{
    [Theory]
    [AutoDomainData]
    public void EnqueueLatentPacket_SinglePlayer_ShouldOneBodyPacketAndOneTailPacket(
        LatentPacketService sut,
        LightTestMtaServer server,
        TestPacketContext context,
        TestTimerService timerService
    )
    {
        // Arrange
        var player = server.CreatePlayer();
        var data = new byte[100];  // Small data (< default rate of 50000)
        ushort resourceNetId = 1;

        // Act
        sut.EnqueueLatentPacket([player], PacketId.PACKET_ID_LUA_EVENT, data, resourceNetId);
        timerService.TriggerTimers();

        // Assert
        context.VerifyPacketSent(PacketId.PACKET_ID_LATENT_TRANSFER, player, count: 1);

        // Act round 2
        timerService.TriggerTimers();
        timerService.TriggerTimers();

        // Assert round 2
        context.VerifyPacketSent(PacketId.PACKET_ID_LATENT_TRANSFER, player, count: 2);
    }

    [Theory]
    [AutoDomainData]
    public void EnqueueLatentPacket_MultiplePlayersButSmallpackets_ShouldProcessOneBodyPacketAndOneTailPacketPerPlayer(
        Configuration configuration,
        LatentPacketService sut,
        LightTestMtaServer server,
        TestPacketContext context,
        TestTimerService timerService
    )
    {
        // Arrange
        var players = new[] {
            server.CreatePlayer(),
            server.CreatePlayer(),
            server.CreatePlayer()
        };
        var data = new byte[100];
        ushort resourceNetId = 1;

        // With test config: bytesPerSend = 10000/1000 * 100 = 1000 bytes

        var bytesPerSend = configuration.LatentBandwidthLimit / 1000 * configuration.LatentSendInterval;
        Assert.Equal(1000u, bytesPerSend);

        // Act
        sut.EnqueueLatentPacket(players, PacketId.PACKET_ID_LUA_EVENT, data, resourceNetId);
        timerService.TriggerTimers();

        // Assert
        context.GetPacketCount(PacketId.PACKET_ID_LATENT_TRANSFER).Should().Be(3);

        foreach (var player in players)
            context.GetPacketCount(PacketId.PACKET_ID_LATENT_TRANSFER, player).Should().Be(1);


        // Act round 2
        timerService.TriggerTimers();
        timerService.TriggerTimers();

        // Assert round 2
        context.GetPacketCount(PacketId.PACKET_ID_LATENT_TRANSFER).Should().Be(6);

        foreach (var player in players)
            context.GetPacketCount(PacketId.PACKET_ID_LATENT_TRANSFER, player).Should().Be(2);
    }

    [Theory]
    [AutoDomainData]
    public void EnqueueLatentPacket_WithCustomRate_ShouldSendPacketDivded(
        LatentPacketService sut,
        LightTestMtaServer server,
        TestPacketContext context,
        TestTimerService timerService
    )
    {
        // Arrange
        var player = server.CreatePlayer();
        var data = new byte[100];
        ushort resourceNetId = 1;
        int customRate = 50;

        // Act
        sut.EnqueueLatentPacket([player], PacketId.PACKET_ID_LUA_EVENT, data, resourceNetId, customRate);
        timerService.TriggerTimers();
        timerService.TriggerTimers();
        timerService.TriggerTimers();

        // Assert
        // The tail is always sent in a different packet from the head, but can be combined with a normal data packet.
        var expectedPacketCount = data.Length / customRate;
        context.VerifyPacketSent(PacketId.PACKET_ID_LATENT_TRANSFER, player, count: expectedPacketCount);
    }

    [Theory]
    [AutoDomainData]
    public void EnqueueLatentPacket_EmptyPlayerList_ShouldNotSendPackets(
        LatentPacketService sut,
        TestPacketContext context,
        TestTimerService timerService
    )
    {
        // Arrange
        var data = new byte[100];
        ushort resourceNetId = 1;

        // Act
        sut.EnqueueLatentPacket([], PacketId.PACKET_ID_LUA_EVENT, data, resourceNetId);
        timerService.TriggerTimers();

        // Assert
        context.VerifyNoPacketsSent();
    }

    [Theory]
    [AutoDomainData]
    public void EnqueueLatentPacket_WithPacketObject_ShouldSendPacket(
        LatentPacketService sut,
        LightTestMtaServer server,
        TestPacketContext context,
        TestTimerService timerService
    )
    {
        // Arrange
        var player = server.CreatePlayer();
        var packet = new LuaEventPacket("testEvent", Packets.Structs.ElementId.Zero, []);
        ushort resourceNetId = 1;

        // Act
        sut.EnqueueLatentPacket([player], packet, resourceNetId);
        timerService.TriggerTimers();

        // Assert
        context.VerifyPacketSent(PacketId.PACKET_ID_LATENT_TRANSFER, player, count: 1);
    }
}
