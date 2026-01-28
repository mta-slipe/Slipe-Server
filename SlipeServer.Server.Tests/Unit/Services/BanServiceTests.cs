using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SlipeServer.Server.Bans;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using SlipeServer.Server.Tests.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Services;

public class BanServiceTests
{
    [Theory]
    [AutoDomainData]
    public void GetBans_ShouldReturnAllBans(
        [Frozen] Mock<IBanRepository> banRepositoryMock,
        BanService sut
    )
    {
        // Arrange
        var expectedBans = new List<Ban>
        {
            new Ban { Id = Guid.NewGuid(), Serial = "TEST123" },
            new Ban { Id = Guid.NewGuid(), IPAddress = IPAddress.Parse("127.0.0.1") }
        };

        banRepositoryMock.Setup(x => x.GetBans()).Returns(expectedBans);

        // Act
        var result = sut.GetBans();

        // Assert
        result.Should().BeEquivalentTo(expectedBans);
        banRepositoryMock.Verify(x => x.GetBans(), Times.Once);
    }

    [Theory]
    [AutoDomainData]
    public void AddBan_WithSerial_ShouldAddBanToRepository(
        [Frozen] Mock<IBanRepository> banRepositoryMock,
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        BanService sut
    )
    {
        // Arrange
        var serial = "TEST123";
        var expectedBan = new Ban { Id = Guid.NewGuid(), Serial = serial };

        banRepositoryMock.Setup(x => x.AddBan(serial, null, null, null, null, null))
            .Returns(expectedBan);
        elementCollectionMock.Setup(x => x.GetByType<Player>())
            .Returns(Enumerable.Empty<Player>());

        // Act
        var result = sut.AddBan(serial, null);

        // Assert
        result.Should().Be(expectedBan);
        banRepositoryMock.Verify(x => x.AddBan(serial, null, null, null, null, null), Times.Once);
    }

    [Theory]
    [AutoDomainData]
    public void AddBan_WithIPAddress_ShouldAddBanToRepository(
        [Frozen] Mock<IBanRepository> banRepositoryMock,
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        BanService sut
    )
    {
        // Arrange
        var ipAddress = IPAddress.Parse("192.168.1.1");
        var expectedBan = new Ban { Id = Guid.NewGuid(), IPAddress = ipAddress };

        banRepositoryMock.Setup(x => x.AddBan(null, ipAddress, null, null, null, null))
            .Returns(expectedBan);
        elementCollectionMock.Setup(x => x.GetByType<Player>())
            .Returns(Enumerable.Empty<Player>());

        // Act
        var result = sut.AddBan(null, ipAddress);

        // Assert
        result.Should().Be(expectedBan);
        banRepositoryMock.Verify(x => x.AddBan(null, ipAddress, null, null, null, null), Times.Once);
    }

    [Theory]
    [AutoDomainData]
    public void AddBan_WithAllParameters_ShouldAddBanToRepository(
        [Frozen] Mock<IBanRepository> banRepositoryMock,
        [Frozen] Mock<IElementCollection> elementCollectionMock,
        BanService sut
    )
    {
        // Arrange
        var serial = "TEST123";
        var ipAddress = IPAddress.Parse("192.168.1.1");
        var expiry = DateTime.UtcNow.AddDays(7);
        var reason = "Test ban";
        var playerName = "TestPlayer";
        var bannerName = "Admin";
        var expectedBan = new Ban
        {
            Id = Guid.NewGuid(),
            Serial = serial,
            IPAddress = ipAddress,
            ExpiryDateUtc = expiry,
            Reason = reason,
            BannedPlayerName = playerName,
            BannerName = bannerName
        };

        banRepositoryMock.Setup(x => x.AddBan(serial, ipAddress, expiry, reason, playerName, bannerName))
            .Returns(expectedBan);
        elementCollectionMock.Setup(x => x.GetByType<Player>())
            .Returns(Enumerable.Empty<Player>());

        // Act
        var result = sut.AddBan(serial, ipAddress, expiry, reason, playerName, bannerName);

        // Assert
        result.Should().Be(expectedBan);
        banRepositoryMock.Verify(x => x.AddBan(serial, ipAddress, expiry, reason, playerName, bannerName), Times.Once);
    }

    [Theory]
    [AutoDomainData]
    public void RemoveBans_WithSerialAndIP_ShouldCallRepository(
        [Frozen] Mock<IBanRepository> banRepositoryMock,
        BanService sut
    )
    {
        // Arrange
        var serial = "TEST123";
        var ipAddress = IPAddress.Parse("192.168.1.1");

        // Act
        sut.RemoveBans(serial, ipAddress);

        // Assert
        banRepositoryMock.Verify(x => x.RemoveBans(serial, ipAddress), Times.Once);
    }

    [Theory]
    [AutoDomainData]
    public void RemoveBan_WithBanObject_ShouldCallRepository(
        [Frozen] Mock<IBanRepository> banRepositoryMock,
        BanService sut
    )
    {
        // Arrange
        var ban = new Ban { Id = Guid.NewGuid(), Serial = "TEST123" };

        // Act
        sut.RemoveBan(ban);

        // Assert
        banRepositoryMock.Verify(x => x.RemoveBan(ban), Times.Once);
    }

    [Theory]
    [AutoDomainData]
    public void RemoveBan_WithGuid_ShouldCallRepository(
        [Frozen] Mock<IBanRepository> banRepositoryMock,
        BanService sut
    )
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        sut.RemoveBan(id);

        // Assert
        banRepositoryMock.Verify(x => x.RemoveBan(id), Times.Once);
    }

    [Theory]
    [AutoDomainData]
    public void IsIpOrSerialBanned_WithBannedSerial_ShouldReturnTrue(
        [Frozen] Mock<IBanRepository> banRepositoryMock,
        BanService sut
    )
    {
        // Arrange
        var serial = "BANNED123";
        var ban = new Ban { Id = Guid.NewGuid(), Serial = serial };

        banRepositoryMock.Setup(x => x.IsIpOrSerialBanned(serial, null, out It.Ref<Ban?>.IsAny))
            .Returns(new BanRepositoryIsIpOrSerialBannedCallback((string? s, IPAddress? ip, out Ban? b) =>
            {
                b = ban;
                return true;
            }));

        // Act
        var result = sut.IsIpOrSerialBanned(serial, null, out var resultBan);

        // Assert
        result.Should().BeTrue();
        resultBan.Should().Be(ban);
    }

    [Theory]
    [AutoDomainData]
    public void IsIpOrSerialBanned_WithNotBannedSerial_ShouldReturnFalse(
        [Frozen] Mock<IBanRepository> banRepositoryMock,
        BanService sut
    )
    {
        // Arrange
        var serial = "NOTBANNED123";

        banRepositoryMock.Setup(x => x.IsIpOrSerialBanned(serial, null, out It.Ref<Ban?>.IsAny))
            .Returns(new BanRepositoryIsIpOrSerialBannedCallback((string? s, IPAddress? ip, out Ban? b) =>
            {
                b = null;
                return false;
            }));

        // Act
        var result = sut.IsIpOrSerialBanned(serial, null, out var resultBan);

        // Assert
        result.Should().BeFalse();
        resultBan.Should().BeNull();
    }

    private delegate bool BanRepositoryIsIpOrSerialBannedCallback(string? serial, IPAddress? ipAddress, out Ban? ban);
}
