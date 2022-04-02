using SlipeServer.Packets.Definitions.Fire;
using SlipeServer.Server.Services;
using SlipeServer.Server.TestTools;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Services;

public class FireServiceTests
{
    [Fact]
    public void CreateFire_BroadcastsFirePacket()
    {
        var mtaServer = new TestingServer();
        var players = new TestingPlayer[] {
            mtaServer.AddFakePlayer(),
            mtaServer.AddFakePlayer(),
            mtaServer.AddFakePlayer(),
        };

        var service = new FireService(mtaServer);
        service.CreateFire(Vector3.Zero);

        foreach (var player in players)
            mtaServer.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_FIRE, player);
    }

    [Fact]
    public void CreateFireFor_SendsPacketsToChosenClients()
    {
        var mtaServer = new TestingServer();
        var players = new TestingPlayer[] {
            mtaServer.AddFakePlayer(),
            mtaServer.AddFakePlayer(),
            mtaServer.AddFakePlayer(),
        };
        var additionalPlayers = new TestingPlayer[]
        {
            mtaServer.AddFakePlayer(),
            mtaServer.AddFakePlayer(),
            mtaServer.AddFakePlayer(),
        };

        var service = new FireService(mtaServer);
        service.CreateFireFor(players, Vector3.Zero);

        foreach (var player in players)
            mtaServer.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_FIRE, player);
        foreach (var player in additionalPlayers)
            mtaServer.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_FIRE, player, count: 0);
    }

    [Fact]
    public void CreateFire_SendsExpectedPacket()
    {
        var mtaServer = new TestingServer();
        var player = mtaServer.AddFakePlayer();

        var service = new FireService(mtaServer);
        service.CreateFire(Vector3.Zero, 2);

        var expectedPacket = new FirePacket(Vector3.Zero, 2);
        mtaServer.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_FIRE, player, expectedPacket.Write());
    }
}
