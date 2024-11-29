﻿using FluentAssertions;
using FluentAssertions.Execution;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.TestTools;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Elements.Player;

public class PlayerTests
{
    [Fact]
    public void PlayerSpawn_ShouldResetDefaults()
    {
        var player = TestingPlayer.CreateStandalone();

        player.HasJetpack = true;
        player.Health = 50;
        player.Armor = 10;
        player.AddWeapon(WeaponId.Ak47, 100);
        player.Dimension = 10;
        player.Interior = 10;
        player.VehicleAction = VehicleAction.Entering;
        player.Vehicle = new SlipeServer.Server.Elements.Vehicle(SlipeServer.Server.Elements.VehicleModel.Alpha, Vector3.Zero);
        player.Seat = 0;

        player.Spawn(Vector3.Zero, 0, 0, 5, 5);

        player.HasJetpack.Should().BeFalse();
        player.Health.Should().Be(100);
        player.Armor.Should().Be(0);
        player.Weapons.Should().BeEmpty();
        player.Dimension.Should().Be(5);
        player.Interior.Should().Be(5);
        player.VehicleAction.Should().Be(VehicleAction.None);
        player.Vehicle.Should().BeNull();
        player.Seat.Should().BeNull();
    }

    [Fact]
    public void SetMoney_ShouldStayWithinBounds()
    {
        var player = TestingPlayer.CreateStandalone();

        player.SetMoney(1000000000);

        player.Money.Should().Be(99999999);
    }

    [Fact]
    public void SetMoney_ShouldStayWithinNegativeBounds()
    {
        var player = TestingPlayer.CreateStandalone();

        player.SetMoney(-1000000000);

        player.Money.Should().Be(-99999999);
    }

    [Fact]
    public void SetMoneyByProperty_ShouldStayWithinBounds()
    {
        var player = TestingPlayer.CreateStandalone();

        player.Money = 1000000000;

        player.Money.Should().Be(99999999);
    }

    [Fact]
    public void SetMoneyByProperty_ShouldStayWithinNegativeBounds()
    {
        var player = TestingPlayer.CreateStandalone();

        player.Money = -1000000000;

        player.Money.Should().Be(-99999999);
    }

    [Fact]
    public void DestroyingVehiclesWithPlayersInsideShouldWork()
    {
        var player = TestingPlayer.CreateStandalone();
        var vehicle = new Vehicle(404, Vector3.Zero);
        
        player.WarpIntoVehicle(vehicle);
        var veh1 = player.Vehicle;
        vehicle.Destroy();
        var veh2 = player.Vehicle;

        using var _ = new AssertionScope();
        veh1.Should().Be(vehicle);
        veh2.Should().BeNull();
    }
}
