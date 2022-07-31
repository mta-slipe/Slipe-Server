using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Relayers;
using SlipeServer.Server.TestTools;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Relayers;

public class PacketRelayerTests
{
    [Fact]
    public void ElementPositionChanged_WithRelayer_RelaysPacket()
    {
        var server = new TestingServer();

        var player = server.AddFakePlayer();
        var relayer = new ConfigurablePropertyRelayer<Element, ElementChangedEventArgs<Vector3>>(
            (element, handler) => element.PositionChanged += (sender, args) => handler(sender, args),
            (element, args) => ElementPacketFactory.CreateSetPositionPacket(element, args.NewValue),
            server);

        var dummy = new DummyElement().AssociateWith(server);
        dummy.Position = new Vector3(10, 20, 30);

        var expectedPacket = new SetElementPositionRpcPacket(dummy.Id, dummy.TimeContext, new Vector3(10, 20, 30)).Write();

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_ELEMENT_RPC, player, expectedPacket);
    }

    [Fact]
    public void ElementPositionChanged_WithoutRelayer_DoesNotRelayPacket()
    {
        var server = new TestingServer();

        var player = server.AddFakePlayer();

        var dummy = new DummyElement().AssociateWith(server);
        dummy.Position = new Vector3(10, 20, 30);

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_ELEMENT_RPC, player, null, 0);
    }
}
