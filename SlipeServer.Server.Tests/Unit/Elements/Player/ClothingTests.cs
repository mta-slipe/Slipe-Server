using FluentAssertions;
using SlipeServer.Server.Constants;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Elements.Player;

public class ClothingTests
{
    [Fact]
    public void TestIfAllDefaultClothesAreInSet()
    {
        ClothesConstants.ShirtsCount.Should().Be(68);
        ClothesConstants.HeadsCount.Should().Be(33);
        ClothesConstants.TrousersCount.Should().Be(45);
        ClothesConstants.ShoesCount.Should().Be(38);
        ClothesConstants.TattoosLeftUpperArmCount.Should().Be(3);
        ClothesConstants.TattoosLeftLowerArmCount.Should().Be(4);
        ClothesConstants.TattoosRightUpperArmCount.Should().Be(4);
        ClothesConstants.TattoosRightLowerArmCount.Should().Be(4);
        ClothesConstants.TattoosBackCount.Should().Be(7);
        ClothesConstants.TattoosLeftChestCount.Should().Be(6);
        ClothesConstants.TattoosRightChestCount.Should().Be(7);
        ClothesConstants.TattoosStomachCount.Should().Be(7);
        ClothesConstants.TattoosLowerBackCount.Should().Be(6);
        ClothesConstants.NecklaceCount.Should().Be(12);
        ClothesConstants.WatchesCount.Should().Be(12);
        ClothesConstants.GlassesCount.Should().Be(17);
        ClothesConstants.HatsCount.Should().Be(57);
        ClothesConstants.ExtraCount.Should().Be(9);
    }
}
