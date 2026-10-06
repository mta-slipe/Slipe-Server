using FluentAssertions;
using Moq;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;
using SlipeServer.Server.TestTools;
using System.Linq;
using System.Numerics;
using Xunit;
using ServerVehicle = SlipeServer.Server.Elements.Vehicle;
using VehicleModel = SlipeServer.Server.Elements.VehicleModel;

namespace SlipeServer.Server.Tests.Integration.PacketHandlers;

public class VehicleDamageSyncPacketHandlerTests
{
    private static VehicleDamageSyncPacketHandler CreateHandler(
        TestingServer server, TestingPlayer sourcePlayer, TestingPlayer[] otherPlayers,
        FlatElementCollection elementCollection, out Mock<ISyncHandlerMiddleware<VehicleDamageSyncPacket>> middlewareMock)
    {
        middlewareMock = new();
        middlewareMock
            .Setup(x => x.GetPlayersToSyncTo(sourcePlayer, It.IsAny<VehicleDamageSyncPacket>()))
            .Returns(otherPlayers);

        return new VehicleDamageSyncPacketHandler(middlewareMock.Object, elementCollection);
    }

    [Fact]
    public void HandlePacketRelaysServerDamageState()
    {
        var server = new TestingServer();
        var sourcePlayer = server.AddFakePlayer();
        var otherPlayers = new[] { server.AddFakePlayer(), server.AddFakePlayer(), server.AddFakePlayer() };

        var vehicle = new ServerVehicle(VehicleModel.Alpha, Vector3.Zero);
        server.AssociateElement(vehicle);
        vehicle.SetDoorState(VehicleDoor.Hood, VehicleDoorState.ShutDamaged);

        var elementCollection = new FlatElementCollection();
        elementCollection.Add(vehicle);

        var handler = CreateHandler(server, sourcePlayer, otherPlayers, elementCollection, out _);

        handler.HandlePacket(sourcePlayer.Client, new VehicleDamageSyncPacket
        {
            VehicleId = vehicle.Id,
            DoorStates = new byte?[6] { null, null, (byte)VehicleDoorState.Missing, null, null, null },
            WheelStates = new byte?[4],
            PanelStates = new byte?[7],
            LightStates = new byte?[4],
        });

        vehicle.GetDoorState(VehicleDoor.Hood).Should().Be(VehicleDoorState.ShutDamaged);
        vehicle.GetDoorState(VehicleDoor.FrontLeft).Should().Be(VehicleDoorState.Missing);

        // The relayed packet must carry the full server damage state, including the components the client
        // did not send a value for, rather than the raw values the client sent.
        var expected = new VehicleDamageSyncPacket
        {
            VehicleId = vehicle.Id,
            DoorStates = [.. vehicle.Damage.Doors.Select(x => (byte?)x)],
            WheelStates = [.. vehicle.Damage.Wheels.Select(x => (byte?)x)],
            PanelStates = [.. vehicle.Damage.Panels.Select(x => (byte?)x)],
            LightStates = [.. vehicle.Damage.Lights.Select(x => (byte?)x)],
        }.Write();

        foreach (var player in otherPlayers)
            server.VerifyPacketSent(PacketId.PACKET_ID_VEHICLE_DAMAGE_SYNC, player, expected);

        server.VerifyPacketSent(PacketId.PACKET_ID_VEHICLE_DAMAGE_SYNC, sourcePlayer, null, 0);
    }

    [Fact]
    public void HandlePacketDoesNotRelayWhenNoComponentChanged()
    {
        var server = new TestingServer();
        var sourcePlayer = server.AddFakePlayer();
        var otherPlayers = new[] { server.AddFakePlayer() };

        var vehicle = new ServerVehicle(VehicleModel.Alpha, Vector3.Zero);
        server.AssociateElement(vehicle);

        var elementCollection = new FlatElementCollection();
        elementCollection.Add(vehicle);

        var handler = CreateHandler(server, sourcePlayer, otherPlayers, elementCollection, out var middlewareMock);

        handler.HandlePacket(sourcePlayer.Client, new VehicleDamageSyncPacket
        {
            VehicleId = vehicle.Id,
            DoorStates = new byte?[6],
            WheelStates = new byte?[4],
            PanelStates = new byte?[7],
            LightStates = new byte?[4],
        });

        middlewareMock.Verify(x => x.GetPlayersToSyncTo(It.IsAny<SlipeServer.Server.Elements.Player>(), It.IsAny<VehicleDamageSyncPacket>()), Times.Never);
        server.VerifyPacketSent(PacketId.PACKET_ID_VEHICLE_DAMAGE_SYNC, otherPlayers[0], null, 0);
    }

    [Fact]
    public void HandlePacketIgnoresPacketForUnknownVehicle()
    {
        var server = new TestingServer();
        var sourcePlayer = server.AddFakePlayer();
        var otherPlayers = new[] { server.AddFakePlayer() };

        var vehicle = new ServerVehicle(VehicleModel.Alpha, Vector3.Zero);
        server.AssociateElement(vehicle);

        var elementCollection = new FlatElementCollection();

        var handler = CreateHandler(server, sourcePlayer, otherPlayers, elementCollection, out var middlewareMock);

        handler.HandlePacket(sourcePlayer.Client, new VehicleDamageSyncPacket
        {
            VehicleId = vehicle.Id,
            DoorStates = new byte?[6] { null, null, (byte)VehicleDoorState.Missing, null, null, null },
            WheelStates = new byte?[4],
            PanelStates = new byte?[7],
            LightStates = new byte?[4],
        });

        middlewareMock.Verify(x => x.GetPlayersToSyncTo(It.IsAny<SlipeServer.Server.Elements.Player>(), It.IsAny<VehicleDamageSyncPacket>()), Times.Never);
        server.VerifyPacketSent(PacketId.PACKET_ID_VEHICLE_DAMAGE_SYNC, otherPlayers[0], null, 0);
    }
}
