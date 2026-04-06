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
        var textItem = new TextItem(this, ++this.textItemId, text, position.X, position.Y, scale: scale, color: color, alignX: horizontalAlignment, alignY: verticalAlignment, shadowAlpha: shadowAlpha);
        SendTextItemTo(players, textItem);
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

    public TextItem CreateTextItem(
        string text,
        float x,
        float y,
        int priority = 1,
        Color? color = null,
        float scale = 1,
        HorizontalTextAlignment alignX = HorizontalTextAlignment.Left,
        VerticalTextAlignment alignY = VerticalTextAlignment.Top,
        byte shadowAlpha = 0)
    {
        return new TextItem(this, ++this.textItemId, text, x, y, priority, color, scale, alignX, alignY, shadowAlpha);
    }

    public TextDisplay CreateTextDisplay() => new TextDisplay();

    public void DeleteTextItemFor(IEnumerable<Player> players, TextItem item)
    {
        new TextItemPacket(item.Id).SendTo(players);
    }

    public void DeleteTextItemFor(Player player, TextItem item)
    {
        new TextItemPacket(item.Id).SendTo(player);
    }

    public void SendTextItemTo(IEnumerable<Player> players, TextItem item)
    {
        var packet = new TextItemPacket(item.Id, item.Position, item.Text, item.Scale, item.Color, item.Format, item.ShadowAlpha);
        packet.SendTo(players);
    }
}
