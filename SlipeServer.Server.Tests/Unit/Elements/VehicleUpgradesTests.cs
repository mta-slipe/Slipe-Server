using FluentAssertions;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Elements
{
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

    }
}
