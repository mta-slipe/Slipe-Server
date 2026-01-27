using FluentAssertions;
using SlipeServer.Server.Services;
using SlipeServer.Server.Tests.Tools;
using System.Linq;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Services;

public class TextItemServiceTests
{
    [Theory]
    [AutoDomainData]
    public void CreateTextItemFor_SinglePlayer_ShouldSendPacketToPlayer(
        TextItemService sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        var position = new Vector2(100, 200);

        var result = sut.CreateTextItemFor(player, "Test Text", position);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0ul);
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_TEXT_ITEM, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void CreateTextItemFor_MultiplePlayers_ShouldSendPacketToAllPlayers(
        TextItemService sut,
        LightTestPlayer[] players,
        TestPacketContext context
    )
    {
        var position = new Vector2(100, 200);

        var result = sut.CreateTextItemFor(players, "Test Text", position);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0ul);

        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_TEXT_ITEM, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void CreateTextItemFor_ShouldIncrementId(
        TextItemService sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        var position = new Vector2(100, 200);

        var item1 = sut.CreateTextItemFor(player, "Text 1", position);
        var item2 = sut.CreateTextItemFor(player, "Text 2", position);
        var item3 = sut.CreateTextItemFor(player, "Text 3", position);

        item2.Id.Should().Be(item1.Id + 1);
        item3.Id.Should().Be(item2.Id + 1);
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_TEXT_ITEM, player, count: 3);
    }

    [Theory]
    [AutoDomainData]
    public void CreateTextItemFor_WithEmptyPlayerList_ShouldNotSendPacket(
        TextItemService sut,
        TestPacketContext context
    )
    {
        var position = new Vector2(100, 200);

        var result = sut.CreateTextItemFor(Enumerable.Empty<LightTestPlayer>(), "Test", position);

        result.Should().NotBeNull();
        context.VerifyNoPacketsSent();
    }

    [Theory]
    [AutoDomainData]
    public void DeleteTextItemFor_SinglePlayer_ShouldSendPacketToPlayer(
        TextItemService sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        var position = new Vector2(100, 200);
        var item = sut.CreateTextItemFor(player, "Test", position);
        context.ResetPacketCountVerification();

        sut.DeleteTextItemFor(player, item);

        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_TEXT_ITEM, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void DeleteTextItemFor_MultiplePlayers_ShouldSendPacketToAllPlayers(
        TextItemService sut,
        LightTestPlayer[] players,
        TestPacketContext context
    )
    {
        var position = new Vector2(100, 200);
        var item = sut.CreateTextItemFor(players, "Test", position);
        context.ResetPacketCountVerification();

        sut.DeleteTextItemFor(players, item);

        foreach (var player in players)
        {
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_TEXT_ITEM, player, count: 1);
        }
    }

    [Theory]
    [AutoDomainData]
    public void CreateTextItemFor_MultipleCallsShouldReturnUniqueIds(
        TextItemService sut,
        LightTestPlayer player
    )
    {
        var position = new Vector2(100, 200);

        var items = Enumerable.Range(0, 100)
            .Select(i => sut.CreateTextItemFor(player, $"Text {i}", position))
            .ToList();

        items.Select(i => i.Id).Should().OnlyHaveUniqueItems();
    }
}
