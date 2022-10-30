using FluentAssertions;
using SlipeServer.Packets.Enums.VehicleUpgrades;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace SlipeServer.Server.Tests.Unit.Elements;

public class VehicleUpgradesTests
{
    [Theory]
    [InlineData(560, typeof(VehicleUpgradeSpoiler), (ushort)VehicleUpgradeSpoiler.XFlow, 1139)] // Sultan
    [InlineData(562, typeof(VehicleUpgradeSpoiler), (ushort)VehicleUpgradeSpoiler.XFlow, 1146)] // Elegy
    [InlineData(509, typeof(VehicleUpgradeSpoiler), (ushort)VehicleUpgradeSpoiler.XFlow, null)] // Bike
    public void UpgradeVehicleUpgradeIdTest(ushort model, Type upgradeType, ushort upgrade, int? expectedValue)
    {
        ushort? upgradeId = VehicleConstants.UpgradeVehicleUpgradeId(upgradeType, model, upgrade);

        upgradeId.Should().Be((ushort?)expectedValue);
    }

    [Theory]
    [InlineData(560, VehicleUpgradeSpoiler.XFlow, VehicleUpgradeSpoiler.XFlow)] // Sultan
    [InlineData(509, VehicleUpgradeSpoiler.XFlow, VehicleUpgradeSpoiler.None)] // Bike
    public void TestVehicleUpgrades(ushort model, VehicleUpgradeSpoiler spoiler, VehicleUpgradeSpoiler expectedSpoiler)
    {
        VehicleUpgradeSpoiler newUpgrade = VehicleUpgradeSpoiler.None;
        Vehicle vehicle = new Vehicle(model, Vector3.Zero);
        vehicle.Upgrades.UpgradeChanged += (Vehicle sender, Server.Elements.Events.VehicleUpgradeChanged e) =>
        {
            newUpgrade = (VehicleUpgradeSpoiler)e.NewUpgrade;
        };
        vehicle.Upgrades.Spoiler = spoiler;
        newUpgrade.Should().Be(expectedSpoiler);
    }

    [Theory]
    [InlineData(560, VehicleUpgradeSpoiler.XFlow, true)] // Sultan
    [InlineData(509, VehicleUpgradeSpoiler.XFlow, false)] // Bike
    public void TestVehicleUpgradesWithCanHave(ushort model, VehicleUpgradeSpoiler spoiler, bool canHaveThatUpgrade)
    {
        Vehicle vehicle = new Vehicle(model, Vector3.Zero);
        vehicle.Upgrades.CanHave(spoiler).Should().Be(canHaveThatUpgrade);
    }

    [Theory]
    [InlineData(560, true, true)] // Sultan
    [InlineData(606, true, true)] // Luggage Trailer A
    [InlineData(509, false, false)] // Bike
    [InlineData(446, false, false)] // Squalo
    public void TestVehicleCanHaveNitroOrHydraulics(ushort model, bool canHaveHydraulics, bool canHaveStereo)
    {
        Vehicle vehicle = new Vehicle(model, Vector3.Zero);
        vehicle.Upgrades.CanHaveHydraulics().Should().Be(canHaveHydraulics);
        vehicle.Upgrades.CanHaveStereo().Should().Be(canHaveStereo);
    }
}
