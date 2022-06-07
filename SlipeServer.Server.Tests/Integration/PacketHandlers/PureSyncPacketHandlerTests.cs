using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.PacketHandling.Handlers.Player.Sync;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.TestTools;
using System;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.PacketHandlers;

public class PureSyncPacketHandlerTests
{
    [Fact]
    public void HandlePacketRelaysPureSyncPacket()
    {
        var server = new TestingServer();
        var sourcePlayer = server.AddFakePlayer();
        var otherPlayers = new TestingPlayer[] { server.AddFakePlayer(), server.AddFakePlayer(), server.AddFakePlayer() };

        Mock<ILogger> loggerMock = new();

        Mock<ISyncHandlerMiddleware<PlayerPureSyncPacket>> middlewareMock = new();
        middlewareMock.Setup(x => x.GetPlayersToSyncTo(sourcePlayer, It.IsAny<PlayerPureSyncPacket>())).Returns(otherPlayers);

        Mock<IElementCollection> elementCollectionMock = new();

        var handler = new PlayerPureSyncPacketHandler(loggerMock.Object, middlewareMock.Object, elementCollectionMock.Object);

        handler.HandlePacket(sourcePlayer.Client, new PlayerPureSyncPacket()
        {

        });

        foreach (var player in otherPlayers)
            server.VerifyPacketSent(PacketId.PACKET_ID_PLAYER_PURESYNC, player);
    }

    [Fact]
    public void HandlePacketSendReturnSyncPacket()
    {
        var server = new TestingServer();
        var sourcePlayer = server.AddFakePlayer();
        var otherPlayers = new TestingPlayer[] { server.AddFakePlayer(), server.AddFakePlayer(), server.AddFakePlayer() };

        Mock<ILogger> loggerMock = new();

        Mock<ISyncHandlerMiddleware<PlayerPureSyncPacket>> middlewareMock = new();
        middlewareMock.Setup(x => x.GetPlayersToSyncTo(sourcePlayer, It.IsAny<PlayerPureSyncPacket>())).Returns(otherPlayers);

        Mock<IElementCollection> elementCollectionMock = new();

        var handler = new PlayerPureSyncPacketHandler(loggerMock.Object, middlewareMock.Object, elementCollectionMock.Object);

        handler.HandlePacket(sourcePlayer.Client, new PlayerPureSyncPacket()
        {

        });

        server.VerifyPacketSent(PacketId.PACKET_ID_RETURN_SYNC, sourcePlayer);
    }

    [Fact]
    public void HandlePacketAppliesSyncData()
    {
        var server = new TestingServer();
        var sourcePlayer = server.AddFakePlayer();

        Mock<ILogger> loggerMock = new();
        Mock<ISyncHandlerMiddleware<PlayerPureSyncPacket>> middlewareMock = new();
        Mock<IElementCollection> elementCollectionMock = new();

        var handler = new PlayerPureSyncPacketHandler(loggerMock.Object, middlewareMock.Object, elementCollectionMock.Object);

        handler.HandlePacket(sourcePlayer.Client, new PlayerPureSyncPacket()
        {
            Position = new Vector3(10, 10, 10),
            Rotation = 0.25f * MathF.PI,
            Health = 50,
            Armor = 75,
            WeaponSlot = 1
        });

        sourcePlayer.Position.Should().Be(new Vector3(10, 10, 10));
        sourcePlayer.PedRotation.Should().Be(45);
        sourcePlayer.Rotation.Should().Be(new Vector3(0, 0, 45));
        sourcePlayer.Health.Should().Be(50);
        sourcePlayer.Armor.Should().Be(75);
        sourcePlayer.CurrentWeaponSlot = WeaponSlot.Melee;
    }
}
