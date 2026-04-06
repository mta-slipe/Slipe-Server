using SlipeServer.Server.Concepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.Services;

public interface ITextItemService
{
    TextItem CreateTextItemFor(IEnumerable<Player> players, string text, Vector2 position, float scale = 1, Color? color = null, byte shadowAlpha = 0, HorizontalTextAlignment horizontalAlignment = HorizontalTextAlignment.Left, VerticalTextAlignment verticalAlignment = VerticalTextAlignment.Top);
    TextItem CreateTextItemFor(Player player, string text, Vector2 position, float scale = 1, Color? color = null, byte shadowAlpha = 0, HorizontalTextAlignment horizontalAlignment = HorizontalTextAlignment.Left, VerticalTextAlignment verticalAlignment = VerticalTextAlignment.Top);
    void DeleteTextItemFor(IEnumerable<Player> players, TextItem item);
    void DeleteTextItemFor(Player player, TextItem item);
    TextItem CreateTextItem(string text, float x, float y, int priority = 1, Color? color = null, float scale = 1, HorizontalTextAlignment alignX = HorizontalTextAlignment.Left, VerticalTextAlignment alignY = VerticalTextAlignment.Top, byte shadowAlpha = 0);
    TextDisplay CreateTextDisplay();
    void SendTextItemTo(IEnumerable<Player> players, TextItem item);
}
