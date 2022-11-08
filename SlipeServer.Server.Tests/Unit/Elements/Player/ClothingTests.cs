using FluentAssertions;
using SlipeServer.Server.Constants;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Elements.Player;

public class ClothingTests
{
    [Fact]
    public void TestIfAllDefaultClothesAreInSet()
    {
        ClothingConstants.ShirtsCount.Should().Be(68);
        ClothingConstants.HeadsCount.Should().Be(33);
        ClothingConstants.TrousersCount.Should().Be(45);
        ClothingConstants.ShoesCount.Should().Be(38);
        ClothingConstants.TattoosLeftUpperArmCount.Should().Be(3);
        ClothingConstants.TattoosLeftLowerArmCount.Should().Be(4);
        ClothingConstants.TattoosRightUpperArmCount.Should().Be(4);
        ClothingConstants.TattoosRightLowerArmCount.Should().Be(4);
        ClothingConstants.TattoosBackCount.Should().Be(7);
        ClothingConstants.TattoosLeftChestCount.Should().Be(6);
        ClothingConstants.TattoosRightChestCount.Should().Be(7);
        ClothingConstants.TattoosStomachCount.Should().Be(7);
        ClothingConstants.TattoosLowerBackCount.Should().Be(6);
        ClothingConstants.NecklaceCount.Should().Be(12);
        ClothingConstants.WatchesCount.Should().Be(12);
        ClothingConstants.GlassesCount.Should().Be(17);
        ClothingConstants.HatsCount.Should().Be(57);
        ClothingConstants.ExtraCount.Should().Be(9);
    }
}
