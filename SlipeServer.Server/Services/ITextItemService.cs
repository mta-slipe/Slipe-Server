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
}