using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets.Rpc;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.PacketHandling.Handlers.Rpc;
using SlipeServer.Server.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.PacketHandlers;

public class PlayerJoinTests
{
    private static RpcPacketHandler CreateHandler(TestingServer server)
        => new(
            Mock.Of<ILogger>(),
            server,
            server.GetRequiredService<IRootElement>(),
            server.GetRequiredService<IElementCollection>(),
            new Configuration()
        );

    private static RpcPacket CreateInitialDataStreamPacket()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)RpcFunctions.INITIAL_DATA_STREAM);
        var packet = new RpcPacket();
        packet.Read(builder.Build());
        return packet;
    }

    private static Player JoinPlayer(TestingServer server)
    {
        var player = server.AddFakePlayer();
        server.ResetPacketCountVerification();
        CreateHandler(server).HandlePacket(player.Client, CreateInitialDataStreamPacket());
        return player;
    }

    private record WarpRpc(ElementId PlayerId, ElementId VehicleId, byte Seat, byte TimeContext);

    private static IEnumerable<WarpRpc> GetWarpRpcs(TestingServer server, Player to)
    {
        foreach (var call in server.GetSentPackets(to))
        {
            if (call.PacketId != PacketId.PACKET_ID_LUA_ELEMENT_RPC)
                continue;
            if (call.Data[0] != (byte)ElementRpcFunction.WARP_PED_INTO_VEHICLE)
                continue;

            var reader = new PacketReader([.. call.Data, .. new byte[8]]);
            reader.GetByte();
            yield return new WarpRpc(reader.GetElementId(), reader.GetElementId(), reader.GetByte(), reader.GetByte());
        }
    }

    [Fact]
    public void Join_PlayerInVehicle_DoesNotChangeTimeContext()
    {
        var server = new TestingServer();
        var playerA = server.AddFakePlayer();

        var vehicle = new Vehicle(411, Vector3.Zero);
        vehicle.AssociateWith(server);
        playerA.WarpIntoVehicle(vehicle, 0);

        var timeContextBeforeJoin = playerA.TimeContext;
        JoinPlayer(server);

        playerA.TimeContext.Should().Be(timeContextBeforeJoin);
    }

    [Fact]
    public void Join_PlayerInVehicle_SendsWarpRpcWithCurrentTimeContext()
    {
        var server = new TestingServer();
        var playerA = server.AddFakePlayer();

        var vehicle = new Vehicle(411, Vector3.Zero);
        vehicle.AssociateWith(server);
        playerA.WarpIntoVehicle(vehicle, 0);

        var timeContextBeforeJoin = playerA.TimeContext;
        var playerB = JoinPlayer(server);

        var warpRpcs = GetWarpRpcs(server, playerB).ToArray();
        warpRpcs.Should().HaveCount(1);
        warpRpcs[0].PlayerId.Should().Be(playerA.Id);
        warpRpcs[0].VehicleId.Should().Be(vehicle.Id);
        warpRpcs[0].Seat.Should().Be(0);
        warpRpcs[0].TimeContext.Should().Be(timeContextBeforeJoin);
        warpRpcs[0].TimeContext.Should().Be(playerA.TimeContext);
    }

    [Fact]
    public void Join_PlayerOnFoot_DoesNotSendWarpRpc()
    {
        var server = new TestingServer();
        server.AddFakePlayer();

        var playerB = JoinPlayer(server);

        GetWarpRpcs(server, playerB).Should().BeEmpty();
    }

    private record PlayerListEntry(ElementId PlayerId, byte TimeContext, bool IsInVehicle, ElementId VehicleId, byte Seat);

    /// <summary>
    /// Mirrors the read sequence of the MTA:SA client's CPacketHandler::Packet_PlayerList.
    /// </summary>
    private static List<PlayerListEntry> ReadPlayerList(byte[] data, out int unreadBits)
    {
        var logicalSize = data.Length * 8;
        var reader = new PacketReader([.. data, .. new byte[64]]);

        reader.GetBit();

        var entries = new List<PlayerListEntry>();
        while (logicalSize - reader.Counter >= 8)
        {
            var playerId = reader.GetElementId();
            var timeContext = reader.GetByte();

            var nickLength = reader.GetByte();
            if (nickLength > 0)
                reader.GetStringCharacters(nickLength);

            reader.GetUint16();
            reader.GetUint32();

            reader.GetBit();
            var isSpawned = reader.GetBit();
            var isInVehicle = reader.GetBit();
            reader.GetBit();
            reader.GetBit();
            var hasNametagColor = reader.GetBit();
            reader.GetBit();
            reader.GetBit();

            var nametagLength = reader.GetByte();
            if (nametagLength > 0)
                reader.GetStringCharacters(nametagLength);
            if (hasNametagColor)
                reader.GetBytes(3);

            reader.GetByte();
            reader.GetCompressedUint16();
            if (reader.GetBit())
                reader.GetElementId();

            if (!isSpawned)
                continue;

            var vehicleId = default(ElementId);
            var seat = (byte)0;
            if (isInVehicle)
            {
                vehicleId = reader.GetElementId();
                seat = reader.GetByteCapped(4);
            }
            else
            {
                reader.GetVector3WithZAsFloat();
                reader.GetFloatFromBits(16, -MathF.PI, MathF.PI);
            }

            reader.GetCompressedUint16();
            reader.GetByte();
            reader.GetCompressedByte();
            reader.GetByte();

            for (var i = 0; i < 16; i++)
                if (reader.GetBit())
                    reader.GetWeaponType();

            reader.GetBit();

            entries.Add(new PlayerListEntry(playerId, timeContext, isInVehicle, vehicleId, seat));
        }

        unreadBits = logicalSize - reader.Counter;
        return entries;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PlayerList_MultiplePlayers_IsReadBackByClientReadOrder(bool playerInVehicleFirst)
    {
        var server = new TestingServer();
        var playerA = server.AddFakePlayer();
        var playerB = server.AddFakePlayer();

        var vehicle = new Vehicle(411, Vector3.Zero);
        vehicle.AssociateWith(server);
        playerA.WarpIntoVehicle(vehicle, 0);

        var players = playerInVehicleFirst
            ? new[] { playerA, playerB }
            : new[] { playerB, playerA };
        var data = PlayerPacketFactory.CreatePlayerListPacket(players, true).Write();

        var entries = ReadPlayerList(data, out var unreadBits);

        unreadBits.Should().BeLessThan(8);
        entries.Should().HaveCount(2);
        entries.Select(x => x.PlayerId).Should().Equal(players.Select(x => x.Id));
        entries.Single(x => x.PlayerId == playerA.Id).Should().Be(new PlayerListEntry(playerA.Id, playerA.TimeContext, true, vehicle.Id, 0));
        entries.Single(x => x.PlayerId == playerB.Id).IsInVehicle.Should().BeFalse();
    }
}
