using SlipeServer.Packets.DefinitionsTextITems;
using SlipeServer.Server.Concepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.Services;

/// <summary>
/// Allows you to create and delete text items to be displayed on a client's UI
/// </summary>
public class TextItemService() : ITextItemService
{
    private ulong textItemId;

    public TextItem CreateTextItemFor(
        IEnumerable<Player> players,
        string text,
        Vector2 position,
        float scale = 1,
        Color? color = null,
        byte shadowAlpha = 0,
        HorizontalTextAlignment horizontalAlignment = HorizontalTextAlignment.Left,
        VerticalTextAlignment verticalAlignment = VerticalTextAlignment.Top
    )
    {
        var textItem = new TextItem(++this.textItemId);

        byte format = (byte)(((byte)horizontalAlignment) | ((byte)verticalAlignment));
        var packet = new TextItemPacket(textItem.Id, position, text, scale, color, format, shadowAlpha);
        packet.SendTo(players);

        return textItem;
    }

    public TextItem CreateTextItemFor(
        Player player,
        string text,
        Vector2 position,
        float scale = 1,
        Color? color = null,
        byte shadowAlpha = 0,
        HorizontalTextAlignment horizontalAlignment = HorizontalTextAlignment.Left,
        VerticalTextAlignment verticalAlignment = VerticalTextAlignment.Top
    )
    {
        return this.CreateTextItemFor(new Player[] { player }, text, position, scale, color, shadowAlpha, horizontalAlignment, verticalAlignment);
    }

    public void DeleteTextItemFor(IEnumerable<Player> players, TextItem item)
    {
        new TextItemPacket(item.Id).SendTo(players);
    }

    public void DeleteTextItemFor(Player player, TextItem item)
    {
        new TextItemPacket(item.Id).SendTo(player);
    }
}
