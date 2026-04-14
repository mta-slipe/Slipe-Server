using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Rpc;
using SlipeServer.Packets.Structs;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Structs;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling.Handlers.Rpc;
using SlipeServer.Server.TestTools;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.PacketHandlers;

public class RpcPacketHandlerTests
{
    private static RpcPacketHandler CreateHandler(TestingServer server)
        => new(
            Mock.Of<ILogger>(),
            server,
            server.GetRequiredService<IRootElement>(),
            server.GetRequiredService<IElementCollection>(),
            new Configuration()
        );

    private static RpcPacket CreateStealthKillPacket(ElementId targetId)
    {
        var builder = new PacketBuilder();
        builder.Write((byte)RpcFunctions.REQUEST_STEALTH_KILL);
        builder.Write(targetId);
        var packet = new RpcPacket();
        packet.Read(builder.Build());
        return packet;
    }

    [Fact]
    public void StealthKill_TriggersEventAndKillsTarget()
    {
        var server = new TestingServer();
        var attacker = server.AddFakePlayer();
        attacker.Health = 100;
        attacker.Weapons.Add(new Weapon(WeaponId.Knife, 1));

        var target = new Ped(PedModel.Cj, Vector3.Zero);
        target.AssociateWith(server);
        target.Health = 100;

        Ped? killedTarget = null;
        attacker.StealthKilled += (_, e) => killedTarget = e.Target;

        CreateHandler(server).HandlePacket(attacker.Client, CreateStealthKillPacket(target.Id));

        killedTarget.Should().Be(target);
        target.Health.Should().Be(0);
    }

    [Fact]
    public void StealthKill_DoesNotFire_WhenElementNotFound()
    {
        var server = new TestingServer();
        var attacker = server.AddFakePlayer();
        attacker.Weapons.Add(new Weapon(WeaponId.Knife, 1));

        bool eventFired = false;
        attacker.StealthKilled += (_, _) => eventFired = true;

        var builder = new PacketBuilder();
        builder.Write((byte)RpcFunctions.REQUEST_STEALTH_KILL);
        builder.WriteElementId(9999u);
        var packet = new RpcPacket();
        packet.Read(builder.Build());

        CreateHandler(server).HandlePacket(attacker.Client, packet);

        eventFired.Should().BeFalse();
    }

    [Fact]
    public void StealthKill_DoesNotFire_WhenTargetIsNotPed()
    {
        var server = new TestingServer();
        var attacker = server.AddFakePlayer();
        attacker.Weapons.Add(new Weapon(WeaponId.Knife, 1));

        var vehicle = new Vehicle(411, Vector3.Zero);
        vehicle.AssociateWith(server);

        bool eventFired = false;
        attacker.StealthKilled += (_, _) => eventFired = true;

        CreateHandler(server).HandlePacket(attacker.Client, CreateStealthKillPacket(vehicle.Id));

        eventFired.Should().BeFalse();
    }

    [Fact]
    public void StealthKill_DoesNotFire_WhenAttackerIsDead()
    {
        var server = new TestingServer();
        var attacker = server.AddFakePlayer();
        attacker.Health = 0;
        attacker.Weapons.Add(new Weapon(WeaponId.Knife, 1));

        var target = new Ped(PedModel.Cj, Vector3.Zero);
        target.AssociateWith(server);
        target.Health = 100;

        bool eventFired = false;
        attacker.StealthKilled += (_, _) => eventFired = true;

        CreateHandler(server).HandlePacket(attacker.Client, CreateStealthKillPacket(target.Id));

        eventFired.Should().BeFalse();
        target.Health.Should().Be(100);
    }

    [Fact]
    public void StealthKill_DoesNotFire_WhenTargetIsDead()
    {
        var server = new TestingServer();
        var attacker = server.AddFakePlayer();
        attacker.Weapons.Add(new Weapon(WeaponId.Knife, 1));

        var target = new Ped(PedModel.Cj, Vector3.Zero);
        target.AssociateWith(server);
        target.Health = 0;

        bool eventFired = false;
        attacker.StealthKilled += (_, _) => eventFired = true;

        CreateHandler(server).HandlePacket(attacker.Client, CreateStealthKillPacket(target.Id));

        eventFired.Should().BeFalse();
    }

    [Fact]
    public void StealthKill_DoesNotFire_WhenAttackerHasNoMeleeWeapon()
    {
        var server = new TestingServer();
        var attacker = server.AddFakePlayer();

        var target = new Ped(PedModel.Cj, Vector3.Zero);
        target.AssociateWith(server);
        target.Health = 100;

        bool eventFired = false;
        attacker.StealthKilled += (_, _) => eventFired = true;

        CreateHandler(server).HandlePacket(attacker.Client, CreateStealthKillPacket(target.Id));

        eventFired.Should().BeFalse();
        target.Health.Should().Be(100);
    }

    [Fact]
    public void StealthKill_DoesNotFire_WhenAttackerHasNonKnifeMeleeWeapon()
    {
        var server = new TestingServer();
        var attacker = server.AddFakePlayer();
        attacker.Weapons.Add(new Weapon(WeaponId.Bat, 1));

        var target = new Ped(PedModel.Cj, Vector3.Zero);
        target.AssociateWith(server);
        target.Health = 100;

        bool eventFired = false;
        attacker.StealthKilled += (_, _) => eventFired = true;

        CreateHandler(server).HandlePacket(attacker.Client, CreateStealthKillPacket(target.Id));

        eventFired.Should().BeFalse();
        target.Health.Should().Be(100);
    }

    [Fact]
    public void StealthKill_DoesNotFire_WhenTooFarAway()
    {
        var server = new TestingServer();
        var attacker = server.AddFakePlayer();
        attacker.Weapons.Add(new Weapon(WeaponId.Knife, 1));

        var target = new Ped(PedModel.Cj, new Vector3(100, 0, 0));
        target.AssociateWith(server);
        target.Health = 100;

        bool eventFired = false;
        attacker.StealthKilled += (_, _) => eventFired = true;

        CreateHandler(server).HandlePacket(attacker.Client, CreateStealthKillPacket(target.Id));

        eventFired.Should().BeFalse();
        target.Health.Should().Be(100);
    }
}
