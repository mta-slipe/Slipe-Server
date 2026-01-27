using AutoFixture.Xunit2;
using Moq;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.Tests.Tools;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Behaviour;

public class LightSyncBehaviourTests
{
    [Theory]
    [AutoDomainData]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "sut is called into by the timer service.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "xUnit1026:Theory methods should use all of their parameter", Justification = "sut is called into by the timer service.")]
    public async Task LightSync_ShouldBroadcastToOtherPlayers(
        [Frozen] Mock<IElementCollection> elementCollection,
        [Frozen] Mock<ISyncHandlerMiddleware<LightSyncBehaviour?>> middlewareMock,
        [Frozen] TestTimerService timerService,
        LightSyncBehaviour sut,
        LightTestPlayer[] players,
        TestPacketContext context
    )
    {
        // Arrange
        elementCollection.Setup(x => x.GetByType<Player>(ElementType.Player))
            .Returns(players);

        foreach (var player in players)
        {
            middlewareMock.Setup(x => x.GetPlayersToSyncTo(player, null))
                .Returns(players.Except([player]));
        }

        // Act
        timerService.TriggerTimers();

        // Assert
        foreach (var player in players)
        {
            middlewareMock.Verify(x => x.GetPlayersToSyncTo(player, null), Times.Once);
        }

        foreach (var player in players)
        {
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LIGHTSYNC, player, count: players.Count() - 1);
        }
    }

    [Theory]
    [AutoDomainData]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "sut is called into by the timer service.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "xUnit1026:Theory methods should use all of their parameter", Justification = "sut is called into by the timer service.")]
    public async Task LightSync_ShouldBroadcastOnlyToPlayersReturnedByMiddleware(
        [Frozen] Mock<IElementCollection> elementCollection,
        [Frozen] Mock<ISyncHandlerMiddleware<LightSyncBehaviour?>> middlewareMock,
        [Frozen] TestTimerService timerService,
        LightSyncBehaviour sut,
        LightTestPlayer[] players,
        TestPacketContext context
    )
    {
        // Arrange
        elementCollection.Setup(x => x.GetByType<Player>(ElementType.Player))
            .Returns(players);

        var lightSyncPlayers = players.Take(2);
        var otherPlayers = players.Except(lightSyncPlayers);


        foreach (var player in lightSyncPlayers)
        {
            middlewareMock.Setup(x => x.GetPlayersToSyncTo(player, null))
                .Returns(lightSyncPlayers.Except([player]));
        }

        foreach (var player in otherPlayers)
        {
            middlewareMock.Setup(x => x.GetPlayersToSyncTo(player, null))
                .Returns([]);
        }

        // Act
        timerService.TriggerTimers();

        // Assert
        foreach (var player in players)
            middlewareMock.Verify(x => x.GetPlayersToSyncTo(player, null), Times.Once);

        foreach (var player in lightSyncPlayers)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LIGHTSYNC, player, count: 1);

        foreach (var player in otherPlayers)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LIGHTSYNC, player, count: 0);
    }

    [Theory]
    [AutoDomainData]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "sut is called into by the timer service.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "xUnit1026:Theory methods should use all of their parameter", Justification = "sut is called into by the timer service.")]
    public async Task LightSync_ShouldDoNothingWhenNoPlayersExist(
        [Frozen] Mock<IElementCollection> elementCollection,
        [Frozen] Mock<ISyncHandlerMiddleware<LightSyncBehaviour?>> middlewareMock,
        [Frozen] TestTimerService timerService,
        LightSyncBehaviour sut,
        TestPacketContext context
    )
    {
        // Arrange
        elementCollection.Setup(x => x.GetByType<Player>(ElementType.Player))
            .Returns([]);

        // Act
        timerService.TriggerTimers();

        // Assert
        middlewareMock.Verify(x => x.GetPlayersToSyncTo(It.IsAny<Player>(), It.IsAny<LightSyncBehaviour?>()), Times.Never);

        context.VerifyNoPacketsSent();
    }
}


