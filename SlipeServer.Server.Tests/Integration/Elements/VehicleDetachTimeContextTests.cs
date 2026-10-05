using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Rpc;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling.Handlers.Rpc;
using SlipeServer.Server.TestTools;
using System.Numerics;
using Xunit;
using VehicleInOutHandler = SlipeServer.Server.PacketHandling.Handlers.Vehicle.VehicleInOutPacketHandler;

namespace SlipeServer.Server.Tests.Integration.Elements;

/// <summary>
/// Detaching a ped from a vehicle must not change the sync time context of that ped, since MTA only
/// generates a new sync time context when the clients are able to adopt it. These tests guard
/// against spurious context increments that would cause the client and server context to diverge.
/// </summary>
public class VehicleDetachTimeContextTests
{
    private static RpcPacketHandler CreateRpcHandler(TestingServer server)
        => new(
            Mock.Of<ILogger>(),
            server,
            server.GetRequiredService<IRootElement>(),
            server.GetRequiredService<IElementCollection>(),
            new Configuration()
        );

    private static VehicleInOutHandler CreateVehicleInOutHandler(TestingServer server)
        => new(
            server.GetRequiredService<IElementCollection>(),
            server,
            Mock.Of<ILogger>()
        );

    private static RpcPacket CreateDataStreamPacket()
    {
        var builder = new PacketBuilder();
        builder.Write((byte)RpcFunctions.INITIAL_DATA_STREAM);
        var packet = new RpcPacket();
        packet.Read(builder.Build());
        return packet;
    }

    private static VehicleInOutPacket CreateNotifyJackPacket(Vehicle vehicle, Player player)
    {
        var builder = new PacketBuilder();
        builder.Write(player.Id);
        builder.Write(vehicle.Id);
        builder.WriteCapped((byte)VehicleInOutAction.NotifyJack, 4);

        var packet = new VehicleInOutPacket();
        packet.Read(builder.Build());
        return packet;
    }

    private static (TestingServer server, TestingPlayer player, Vehicle vehicle) CreateSeatedPlayer()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var vehicle = new Vehicle((ushort)VehicleModel.Landstalker, Vector3.Zero).AssociateWith(server);

        player.WarpIntoVehicle(vehicle, 0);

        return (server, player, vehicle);
    }

    [Fact]
    public void KillingPlayerInVehicle_ClearsVehicleOccupant()
    {
        var (_, player, vehicle) = CreateSeatedPlayer();

        vehicle.Occupants.Should().HaveCount(1);

        player.Kill();

        using var _ = new AssertionScope();
        player.Vehicle.Should().BeNull();
        vehicle.Occupants.Should().BeEmpty();
        vehicle.Driver.Should().BeNull();
    }

    [Fact]
    public void KillingPlayerInVehicle_DoesNotGenerateExtraTimeContext()
    {
        var (_, player, _) = CreateSeatedPlayer();

        var contextBefore = player.TimeContext;

        player.Kill();

        // The wasted packet is the only thing that generates a new sync time context.
        player.TimeContext.Should().Be((byte)(contextBefore + 1));
    }

    [Fact]
    public void AddingPassengerAfterPreviousOccupantKilled_DoesNotRemoveDeadPedFromVehicle()
    {
        var (server, player, vehicle) = CreateSeatedPlayer();

        player.Kill();

        var contextBefore = player.TimeContext;
        server.ResetPacketCountVerification();

        var otherPlayer = server.AddFakePlayer();
        vehicle.AddPassenger(0, otherPlayer, true);

        using var _ = new AssertionScope();
        player.TimeContext.Should().Be(contextBefore);
        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.REMOVE_PED_FROM_VEHICLE, player, count: 0);
    }

    [Fact]
    public void LateJoiningPlayer_DoesNotChangeOccupantTimeContext()
    {
        var (server, seated, _) = CreateSeatedPlayer();

        var contextBefore = seated.TimeContext;

        var joiner = server.AddFakePlayer();
        CreateRpcHandler(server).HandlePacket(joiner.Client, CreateDataStreamPacket());

        seated.TimeContext.Should().Be(contextBefore);
    }

    [Fact]
    public void LateJoiningPlayer_IsToldAboutOccupants()
    {
        var (server, seated, _) = CreateSeatedPlayer();

        var joiner = server.AddFakePlayer();
        server.ResetPacketCountVerification();

        CreateRpcHandler(server).HandlePacket(joiner.Client, CreateDataStreamPacket());

        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.WARP_PED_INTO_VEHICLE, joiner);
    }

    [Fact]
    public void RemovingJackedPed_RelaysRemovalToClients()
    {
        var (server, ped, vehicle) = CreateSeatedPlayer();

        ped.VehicleAction = VehicleAction.Jacked;

        var contextBefore = ped.TimeContext;
        server.ResetPacketCountVerification();

        vehicle.RemovePassenger(ped, true);

        using var _ = new AssertionScope();
        ped.TimeContext.Should().Be((byte)(contextBefore + 1));
        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.REMOVE_PED_FROM_VEHICLE, ped);
    }

    [Fact]
    public void JackingPlayer_TossesDriverWithoutRemovingHimFromVehicle()
    {
        var (server, jackedPed, vehicle) = CreateSeatedPlayer();
        var jackingPlayer = server.AddFakePlayer();

        jackedPed.VehicleAction = VehicleAction.Jacked;
        jackingPlayer.VehicleAction = VehicleAction.Jacking;
        vehicle.JackingPed = jackingPlayer;

        var contextBefore = jackedPed.TimeContext;
        server.ResetPacketCountVerification();

        CreateVehicleInOutHandler(server).HandlePacket(
            jackingPlayer.Client,
            CreateNotifyJackPacket(vehicle, jackingPlayer)
        );

        using var _ = new AssertionScope();
        vehicle.Driver.Should().Be(jackingPlayer);
        jackedPed.Vehicle.Should().BeNull();
        jackedPed.TimeContext.Should().Be(contextBefore);
        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.REMOVE_PED_FROM_VEHICLE, jackedPed, count: 0);
    }

    [Fact]
    public void RemovingPedThatIsStillEnteringVehicle_AbortsEntering()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var vehicle = new Vehicle((ushort)VehicleModel.Landstalker, Vector3.Zero).AssociateWith(server);

        player.Seat = 0;
        player.VehicleAction = VehicleAction.Entering;
        player.EnteringVehicle = vehicle;

        var contextBefore = player.TimeContext;
        server.ResetPacketCountVerification();

        player.RemoveFromVehicle();

        using var _ = new AssertionScope();
        player.EnteringVehicle.Should().BeNull();
        player.VehicleAction.Should().Be(VehicleAction.None);
        player.TimeContext.Should().Be(contextBefore);
        server.VerifyPacketSent(PacketId.PACKET_ID_VEHICLE_INOUT, player);
        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.REMOVE_PED_FROM_VEHICLE, player, count: 0);
    }
}
