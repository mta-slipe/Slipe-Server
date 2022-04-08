using SlipeServer.Packets.Definitions.Explosions;
using SlipeServer.Server.Services;
using SlipeServer.Server.TestTools;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Services;

public class ExplosionServiceTests
{
    [Fact]
    public void CreateExplosion_BroadcastsExplosionPacket()
    {
        var mtaServer = new TestingServer();
        var players = new TestingPlayer[] {
            mtaServer.AddFakePlayer(),
            mtaServer.AddFakePlayer(),
            mtaServer.AddFakePlayer(),
        };

        var service = new ExplosionService(mtaServer);
        service.CreateExplosion(Vector3.Zero, Enums.ExplosionType.WeakRocket);

        foreach (var player in players)
            mtaServer.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_EXPLOSION, player);
    }

    [Fact]
    public void CreateExplosionFor_SendsPacketsToChosenClients()
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

        var service = new ExplosionService(mtaServer);
        service.CreateExplosionFor(players, Vector3.Zero, Enums.ExplosionType.WeakRocket);

        foreach (var player in players)
            mtaServer.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_EXPLOSION, player);
        foreach (var player in additionalPlayers)
            mtaServer.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_EXPLOSION, player, count: 0);
    }

    [Fact]
    public void CreateExplosion_SendsExpectedPacket()
    {
        var mtaServer = new TestingServer();
        var player = mtaServer.AddFakePlayer();

        var service = new ExplosionService(mtaServer);
        service.CreateExplosion(Vector3.Zero, Enums.ExplosionType.WeakRocket);

        var expectedPacket = new ExplosionPacket(null, null, Vector3.Zero, (byte)Enums.ExplosionType.WeakRocket, 0);
        mtaServer.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_EXPLOSION, player, expectedPacket.Write());
    }
}
