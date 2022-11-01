using FluentAssertions;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Services;
using SlipeServer.Server.TestTools;
using System;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Services;

public class WeaponConfigurationServiceTests
{
    [Fact]
    public void GetWeaponConfiguration_ReturnsDefaults_WhenUnmodified()
    {
        var mtaServer = new TestingServer();

        var service = new WeaponConfigurationService(mtaServer);

        foreach (var weapon in Enum.GetValues<Enums.WeaponId>())
        {
            foreach (var level in Enum.GetValues<Enums.WeaponSkillLevel>())
            {
                var result = service.GetWeaponConfiguration(weapon, level);

                if (WeaponConfigurationConstants.Defaults.ContainsKey(weapon) && WeaponConfigurationConstants.Defaults[weapon].ContainsKey(level))
                    result.Should().BeEquivalentTo(WeaponConfigurationConstants.Defaults[weapon][level]);
            }
        }
    }

    [Fact]
    public void GetWeaponConfiguration_ReturnsOverwrittenValue_WhenModified()
    {
        var mtaServer = new TestingServer();

        var service = new WeaponConfigurationService(mtaServer);

        var original = service.GetWeaponConfiguration(Enums.WeaponId.Deagle);
        var modified = original;
        modified.MaximumClipAmmo = 15;
        service.SetWeaponConfiguration(Enums.WeaponId.Deagle, modified);

        var result = service.GetWeaponConfiguration(Enums.WeaponId.Deagle);
        result.Should().BeEquivalentTo(modified);
    }

    [Fact]
    public void GetWeaponConfiguration_ReturnsOriginalValue_WhenReturnTypeIsModified()
    {
        var mtaServer = new TestingServer();

        var service = new WeaponConfigurationService(mtaServer);

        var original = service.GetWeaponConfiguration(Enums.WeaponId.Deagle);
        var modified = original;
        modified.MaximumClipAmmo = 15;

        var result = service.GetWeaponConfiguration(Enums.WeaponId.Deagle);
        result.MaximumClipAmmo.Should().Be(7);
    }
}
