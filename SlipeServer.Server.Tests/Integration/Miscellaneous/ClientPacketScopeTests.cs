using Moq;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.PacketHandling;
using SlipeServer.Server.TestTools;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Miscellaneous;

public class ClientPacketScopeTests
{
    [Fact]
    public void ClientInScope_SendsPacket()
    {
        var server = new TestingServer();

        var player = server.AddFakePlayer();

        using var scope = new ClientPacketScope(new Client[] { player.Client });
        player.Client.SendPacket(new SetElementModelRpcPacket(player.Id, 0));

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_ELEMENT_RPC, player);
    }

    [Fact]
    public void ClientOutOfScope_DoesNotSendPacket()
    {
        var server = new TestingServer();

        var player = server.AddFakePlayer();

        using var scope = new ClientPacketScope(Array.Empty<Client>());
        player.Client.SendPacket(new SetElementModelRpcPacket(player.Id, 0));

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_ELEMENT_RPC, player, count: 0);
    }

    [Fact]
    public async Task AsyncScopes_DoNotInterfere()
    {
        var server = new TestingServer();

        var player = server.AddFakePlayer();
        var expectedPacket = new SetElementModelRpcPacket(player.Id, 1);
        var notExpectedPacket = new SetElementModelRpcPacket(player.Id, 2);

        await Task.WhenAll(new Task[]
        {
                Task.Run(async() =>
                {
                    await Task.Delay(10);
                    using var scope = new ClientPacketScope(new Client[] { player.Client });
                    player.Client.SendPacket(expectedPacket);
                    await Task.Delay(25);
                }),
                Task.Run(async() =>
                {
                    using var scope = new ClientPacketScope(Array.Empty<Client>());
                    await Task.Delay(25);
                    player.Client.SendPacket(notExpectedPacket);
                })
        });

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_ELEMENT_RPC, player, expectedPacket.Write(), count: 1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_ELEMENT_RPC, player, notExpectedPacket.Write(), count: 0);
    }
}
