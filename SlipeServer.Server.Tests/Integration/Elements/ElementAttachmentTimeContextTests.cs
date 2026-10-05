using FluentAssertions;
using FluentAssertions.Execution;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Reader;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.TestTools;
using System;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Elements;

/// <summary>
/// Attaching an element to another element and detaching it again has to follow MTA exactly, since
/// the MTA client reads these RPCs field by field and adopts the sync time context that the detach
/// RPC carries. Detaching generates a new context, so a detach RPC without one leaves the client on
/// the old context and makes it reject every following sync from that element.
/// </summary>
public class ElementAttachmentTimeContextTests
{
    private static (TestingServer server, TestingPlayer player, Vehicle vehicle) CreateAttachedPlayer(Vector3? rotationOffset = null)
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var vehicle = new Vehicle((ushort)VehicleModel.Landstalker, Vector3.Zero).AssociateWith(server);

        player.AttachTo(vehicle, Vector3.Zero, rotationOffset);

        return (server, player, vehicle);
    }

    [Fact]
    public void AttachingPlayerToVehicle_DoesNotChangeTimeContext()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var vehicle = new Vehicle((ushort)VehicleModel.Landstalker, Vector3.Zero).AssociateWith(server);

        var contextBefore = player.TimeContext;

        player.AttachTo(vehicle, Vector3.Zero, Vector3.Zero);

        // MTA does not generate a sync time context in AttachElements, and the attach RPC carries none.
        using var _ = new AssertionScope();
        player.Attachment.Should().NotBeNull();
        player.Attachment!.Target.Should().Be(vehicle);
        player.TimeContext.Should().Be(contextBefore);
    }

    [Fact]
    public void AttachElementRpc_DoesNotCarryATimeContext()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        var vehicle = new Vehicle((ushort)VehicleModel.Landstalker, Vector3.Zero).AssociateWith(server);
        server.ResetPacketCountVerification();

        var offsetPosition = new Vector3(1, 2, 3);
        var offsetRotation = new Vector3(10, 20, 30);

        player.AttachTo(vehicle, offsetPosition, offsetRotation);

        // CElementRPCs::AttachElements reads the target id, then the position and rotation offsets.
        // The rotation is converted to radians on the wire.
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.ATTACH_ELEMENTS);
        builder.Write(player.Id);
        builder.Write(vehicle.Id);
        builder.Write(offsetPosition);
        builder.Write(offsetRotation * MathF.PI / 180);

        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.ATTACH_ELEMENTS, player, builder.Build());
    }

    [Fact]
    public void DetachingPlayerFromVehicle_GeneratesExactlyOneTimeContext()
    {
        var (_, player, vehicle) = CreateAttachedPlayer();

        var contextBefore = player.TimeContext;

        player.DetachFrom(vehicle);

        using var _ = new AssertionScope();
        player.Attachment.Should().BeNull();
        player.TimeContext.Should().Be((byte)(contextBefore + 1));
    }

    [Fact]
    public void DetachElementRpc_CarriesTheNewTimeContext()
    {
        var (server, player, vehicle) = CreateAttachedPlayer();
        server.ResetPacketCountVerification();

        player.DetachFrom(vehicle);

        // CElementRPCs::DetachElements reads the sync time context first, then the position and the
        // rotation, and calls SetSyncTimeContext with it. The context has to be the new one, because
        // that is the context the client is expected to use from then on.
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.DETACH_ELEMENTS);
        builder.Write(player.Id);
        builder.Write(player.TimeContext);
        builder.Write(player.Position);
        builder.Write(player.Rotation);

        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.DETACH_ELEMENTS, player, builder.Build());
    }

    [Fact]
    public void DetachElementRpc_HasTheLayoutTheClientReads()
    {
        var (server, player, vehicle) = CreateAttachedPlayer();
        player.Position = new Vector3(11, 12, 13);
        player.Rotation = new Vector3(21, 22, 23);
        server.ResetPacketCountVerification();

        player.DetachFrom(vehicle);

        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.DETACH_ELEMENTS, player);
        var data = server.LastLuaElementRpcPacketData(ElementRpcFunction.DETACH_ELEMENTS, player);

        // CElementRPCs::DetachElements reads, in order: the sync time context, the position and the
        // rotation. Reading the packet back in that order is the only way to end up with the right
        // values, so parsing it this way proves the layout the client expects.
        var reader = new PacketReader(data);

        using var _ = new AssertionScope();
        reader.GetByte().Should().Be((byte)ElementRpcFunction.DETACH_ELEMENTS);
        reader.GetElementId().Should().Be(player.Id);
        reader.GetByte().Should().Be(player.TimeContext);
        new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat()).Should().Be(player.Position);
        new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat()).Should().Be(player.Rotation);
    }

    [Fact]
    public void ChangingAttachedOffsets_RelaysBothThePositionAndTheRotation()
    {
        var (server, player, vehicle) = CreateAttachedPlayer(new Vector3(10, 20, 30));
        server.ResetPacketCountVerification();

        var newPosition = new Vector3(4, 5, 6);
        player.Attachment!.PositionOffset = newPosition;

        // CElementRPCs::SetElementAttachedOffsets reads a position and a rotation, both as floats, and
        // the rotation is in radians. The rotation offset of the attachment must not be replaced by the
        // position offset.
        var builder = new PacketBuilder();
        builder.Write((byte)ElementRpcFunction.SET_ELEMENT_ATTACHED_OFFSETS);
        builder.Write(player.Id);
        builder.Write(newPosition);
        builder.Write(new Vector3(10, 20, 30) * MathF.PI / 180);

        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.SET_ELEMENT_ATTACHED_OFFSETS, player, builder.Build());
    }
}
