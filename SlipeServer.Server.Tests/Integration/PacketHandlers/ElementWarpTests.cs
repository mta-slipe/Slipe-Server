using FluentAssertions;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using SlipeServer.Server.TestTools;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.PacketHandlers;

public class ElementWarpTests
{
    private record PositionRpc(ElementId ElementId, Vector3 Position, byte TimeContext, int UnreadBits);

    /// <summary>
    /// Mirrors the read sequence of the MTA:SA client's CElementRPCs::SetElementPosition. The client reads a
    /// warp byte only if one is present: a packet ending right after the time context is a warp, and a trailing
    /// zero byte means the position change is not a warp.
    /// </summary>
    private static IEnumerable<PositionRpc> GetPositionRpcs(TestingServer server, Player to)
    {
        foreach (var call in server.GetSentPackets(to))
        {
            if (call.PacketId != PacketId.PACKET_ID_LUA_ELEMENT_RPC)
                continue;
            if (call.Data[0] != (byte)ElementRpcFunction.SET_ELEMENT_POSITION)
                continue;

            var reader = new PacketReader([.. call.Data, .. new byte[8]]);
            reader.GetByte();
            var elementId = reader.GetElementId();
            var position = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            var timeContext = reader.GetByte();

            yield return new PositionRpc(elementId, position, timeContext, (call.Data.Length * 8) - reader.Counter);
        }
    }

    [Fact]
    public void SetPosition_OnPlayer_RelaysWarpPositionRpc()
    {
        var server = new TestingServer();
        var playerA = server.AddFakePlayer();
        var playerB = server.AddFakePlayer();

        server.ResetPacketCountVerification();
        playerA.Position = new Vector3(100, 100, 5);

        var toPlayerA = GetPositionRpcs(server, playerA).ToArray();
        var toPlayerB = GetPositionRpcs(server, playerB).ToArray();

        toPlayerA.Should().HaveCount(1);
        toPlayerB.Should().HaveCount(1);

        var rpc = toPlayerB[0];
        rpc.ElementId.Should().Be(playerA.Id);
        rpc.Position.Should().Be(new Vector3(100, 100, 5));
        rpc.TimeContext.Should().Be(playerA.TimeContext);

        // The player is only teleported by the client, and the game world's streaming state only updated,
        // when the warp byte is absent. A trailing byte would leave the player moved in the matrix but not
        // in the world, which is invisible to other players.
        toPlayerA[0].UnreadBits.Should().BeLessThan(8);
        toPlayerB[0].UnreadBits.Should().BeLessThan(8);
    }

    [Fact]
    public void SetPosition_OnVehicle_RelaysWarpPositionRpc()
    {
        var server = new TestingServer();
        var playerA = server.AddFakePlayer();

        var vehicle = new Vehicle(411, Vector3.Zero);
        vehicle.AssociateWith(server);

        server.ResetPacketCountVerification();
        vehicle.Position = new Vector3(100, 100, 5);

        var rpcs = GetPositionRpcs(server, playerA).ToArray();

        rpcs.Should().HaveCount(1);
        rpcs[0].ElementId.Should().Be(vehicle.Id);
        rpcs[0].Position.Should().Be(new Vector3(100, 100, 5));
        rpcs[0].TimeContext.Should().Be(vehicle.TimeContext);
        rpcs[0].UnreadBits.Should().BeLessThan(8);
    }

    [Fact]
    public void SetPosition_FromSync_DoesNotRelayPositionRpc()
    {
        var server = new TestingServer();
        var playerA = server.AddFakePlayer();
        var playerB = server.AddFakePlayer();

        server.ResetPacketCountVerification();
        playerA.RunAsSync(() => playerA.Position = new Vector3(100, 100, 5));

        playerA.Position.Should().Be(new Vector3(100, 100, 5));
        GetPositionRpcs(server, playerB).Should().BeEmpty();
    }
}
