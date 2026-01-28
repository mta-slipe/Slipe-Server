using FluentAssertions;
using SlipeServer.Server.Services;
using SlipeServer.Server.Tests.Tools;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Services;

public class GameWorldTests
{
    [Theory]
    [AutoDomainData]
    public void GetGameSpeed_ShouldReturnSetValue(
        GameWorld sut
    )
    {
        // Act
        sut.GameSpeed = 1.5f;

        // Assert
        sut.GameSpeed.Should().Be(1.5f);
    }

    [Theory]
    [AutoDomainData]
    public void GetGravity_ShouldReturnSetValue(
        GameWorld sut
    )
    {
        // Act
        sut.Gravity = 0.012f;

        // Assert
        sut.Gravity.Should().Be(0.012f);
    }

    [Theory]
    [AutoDomainData]
    public void GetCloudsEnabled_ShouldReturnSetValue(
        GameWorld sut
    )
    {
        // Act
        sut.CloudsEnabled = false;

        // Assert
        sut.CloudsEnabled.Should().BeFalse();
    }

    [Theory]
    [AutoDomainData]
    public void SetWeather_ShouldUpdateWeatherAndPreviousWeather(
        GameWorld sut
    )
    {
        // Act
        sut.Weather = 5;

        // Assert
        sut.Weather.Should().Be(5);
        sut.PreviousWeather.Should().Be(5);
    }

    [Theory]
    [AutoDomainData]
    public void GetTime_ShouldReturnCurrentTime(
        GameWorld sut
    )
    {
        // Act
        var time = sut.Time;

        // Assert
        time.hour.Should().BeInRange((byte)0, (byte)23);
        time.minute.Should().BeInRange((byte)0, (byte)59);
    }

    [Theory]
    [AutoDomainData]
    public void SetMultipleProperties_ShouldUpdateAllValues(
        GameWorld sut
    )
    {
        // Act
        sut.GameSpeed = 2.0f;
        sut.Gravity = 0.01f;
        sut.RainLevel = 0.5f;

        // Assert
        sut.GameSpeed.Should().Be(2.0f);
        sut.Gravity.Should().Be(0.01f);
        sut.RainLevel.Should().Be(0.5f);
    }
}

