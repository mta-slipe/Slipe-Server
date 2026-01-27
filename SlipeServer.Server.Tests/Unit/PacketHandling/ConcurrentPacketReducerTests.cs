using Moq;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Server.Clients;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.PacketHandling.Reducing;
using SlipeServer.Server.Tests.Tools;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.PacketHandling;

public class ConcurrentPacketReducerTests
{
    [Theory]
    [AutoDomainData]
    public void ConcurrentPacketReducer_EnqueuesMatchingPackets(
        Mock<IPacketQueueHandler<JoinedGamePacket>> handlerMock,
        ConcurrentPacketReducer sut,
        JoinedGamePacket joinedGamePacket
    )
    {
        // Arrange;
        var clientMock = new Mock<IClient>();

        sut.RegisterPacketHandler<JoinedGamePacket>(joinedGamePacket.PacketId, handlerMock.Object);

        // Act
        sut.EnqueuePacket(clientMock.Object, joinedGamePacket.PacketId, []);

        // Assert
        handlerMock.Verify(handler => handler.EnqueuePacket(clientMock.Object, It.IsAny<JoinedGamePacket>()), Times.Once);
    }

    [Theory]
    [AutoDomainData]
    public void ConcurrentPacketReducer_DoesNotQneuueOtherPacketTypes(
        Mock<IPacketQueueHandler<JoinedGamePacket>> handlerMock,
        ConcurrentPacketReducer sut,
        JoinedGamePacket joinedGamePacket
    )
    {
        // Arrange;
        var clientMock = new Mock<IClient>();

        sut.RegisterPacketHandler<JoinedGamePacket>(Packets.Enums.PacketId.PACKET_ID_CAMERA_SYNC, handlerMock.Object);

        // Act
        sut.EnqueuePacket(clientMock.Object, joinedGamePacket.PacketId, []);

        // Assert
        handlerMock.Verify(handler => handler.EnqueuePacket(clientMock.Object, It.IsAny<JoinedGamePacket>()), Times.Never);
    }
}

