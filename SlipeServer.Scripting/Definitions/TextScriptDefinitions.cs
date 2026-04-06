using SlipeServer.Server.Concepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class TextScriptDefinitions(ITextItemService textItemService)
{
    private static int ParsePriority(string priority) => priority switch
    {
        "high" => 2,
        "low" => 0,
        _ => 1,
    };

    private static HorizontalTextAlignment ParseAlignX(string alignX) => alignX switch
    {
        "center" => HorizontalTextAlignment.Center,
        "right" => HorizontalTextAlignment.Right,
        _ => HorizontalTextAlignment.Left,
    };

    private static VerticalTextAlignment ParseAlignY(string alignY) => alignY switch
    {
        "center" => VerticalTextAlignment.Center,
        "bottom" => VerticalTextAlignment.Bottom,
        _ => VerticalTextAlignment.Top,
    };

    [ScriptFunctionDefinition("textCreateDisplay")]
    public TextDisplay TextCreateDisplay() => textItemService.CreateTextDisplay();

    [ScriptFunctionDefinition("textCreateTextItem")]
    public TextItem TextCreateTextItem(
        string text,
        float x,
        float y,
        string priority = "medium",
        int red = 255,
        int green = 255,
        int blue = 255,
        int alpha = 255,
        float scale = 1,
        string alignX = "left",
        string alignY = "top",
        int shadowAlpha = 0)
    {
        return textItemService.CreateTextItem(
            text, x, y,
            ParsePriority(priority),
            Color.FromArgb(alpha, red, green, blue),
            scale,
            ParseAlignX(alignX),
            ParseAlignY(alignY),
            (byte)shadowAlpha);
    }

    [ScriptFunctionDefinition("textDestroyDisplay")]
    public bool TextDestroyDisplay(TextDisplay display)
    {
        display.Destroy();
        return true;
    }

    [ScriptFunctionDefinition("textDestroyTextItem")]
    public bool TextDestroyTextItem(TextItem textItem)
    {
        foreach (var display in textItem.Displays.ToList())
            display.RemoveText(textItem);

        return true;
    }

    [ScriptFunctionDefinition("textDisplayAddObserver")]
    public void TextDisplayAddObserver(TextDisplay display, Player player)
    {
        display.AddObserver(player);
    }

    [ScriptFunctionDefinition("textDisplayAddText")]
    public void TextDisplayAddText(TextDisplay display, TextItem textItem)
    {
        display.AddText(textItem);
    }

    [ScriptFunctionDefinition("textDisplayGetObservers")]
    public IEnumerable<Player> TextDisplayGetObservers(TextDisplay display)
    {
        return display.Observers;
    }

    [ScriptFunctionDefinition("textDisplayIsObserver")]
    public bool TextDisplayIsObserver(TextDisplay display, Player player)
    {
        return display.IsObserver(player);
    }

    [ScriptFunctionDefinition("textDisplayRemoveObserver")]
    public bool TextDisplayRemoveObserver(TextDisplay display, Player player)
    {
        display.RemoveObserver(player);
        return true;
    }

    [ScriptFunctionDefinition("textDisplayRemoveText")]
    public void TextDisplayRemoveText(TextDisplay display, TextItem textItem)
    {
        display.RemoveText(textItem);
    }

    [ScriptFunctionDefinition("textItemGetColor")]
    public Color TextItemGetColor(TextItem textItem) => textItem.Color;

    [ScriptFunctionDefinition("textItemGetPosition")]
    public Vector2 TextItemGetPosition(TextItem textItem) => new Vector2(textItem.X, textItem.Y);

    [ScriptFunctionDefinition("textItemGetPriority")]
    public int TextItemGetPriority(TextItem textItem) => textItem.Priority;

    [ScriptFunctionDefinition("textItemGetScale")]
    public float TextItemGetScale(TextItem textItem) => textItem.Scale;

    [ScriptFunctionDefinition("textItemGetText")]
    public string TextItemGetText(TextItem textItem) => textItem.Text;

    [ScriptFunctionDefinition("textItemSetColor")]
    public bool TextItemSetColor(TextItem textItem, int red, int green, int blue, int alpha)
    {
        textItem.Color = Color.FromArgb(alpha, red, green, blue);
        return true;
    }

    [ScriptFunctionDefinition("textItemSetPosition")]
    public bool TextItemSetPosition(TextItem textItem, float x, float y)
    {
        textItem.X = x;
        textItem.Y = y;
        return true;
    }

    [ScriptFunctionDefinition("textItemSetPriority")]
    public void TextItemSetPriority(TextItem textItem, string priority)
    {
        textItem.Priority = ParsePriority(priority);
    }

    [ScriptFunctionDefinition("textItemSetScale")]
    public bool TextItemSetScale(TextItem textItem, float scale)
    {
        textItem.Scale = scale;
        return true;
    }

    [ScriptFunctionDefinition("textItemSetText")]
    public void TextItemSetText(TextItem textItem, string text)
    {
        textItem.Text = text;
    }
}

