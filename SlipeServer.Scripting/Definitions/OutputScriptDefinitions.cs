using Microsoft.Extensions.Logging;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using System.Drawing;

namespace SlipeServer.Scripting.Definitions;

public class OutputScriptDefinitions(IDebugLog debugLog, ILogger logger, IChatBox chatBox, IClientConsole clientConsole)
{
    [ScriptFunctionDefinition("outputDebugString")]
    public void OutputDebugString(string message, DebugLevel level = DebugLevel.Information, int red = 255, int green = 255, int blue = 255)
    {
        var color = Color.FromArgb(red, green, blue);
        debugLog.Output(message, level, color);
        switch (level)
        {
            case DebugLevel.Information:
                logger.LogInformation(message);
                break;
            case DebugLevel.Warning:
                logger.LogWarning(message);
                break;
            case DebugLevel.Error:
                logger.LogError(message);
                break;
            case DebugLevel.Custom:
            default:
                logger.LogDebug(message);
                break;
        }
    }

    [ScriptFunctionDefinition("outputServerLog")]
    public void OutputServerLog(string message)
    {
        logger.LogDebug(message);
    }

    [ScriptFunctionDefinition("clearChatBox")]
    public void ClearChatBox(Player? player = null)
    {
        if(player == null)
        {
            chatBox.Clear();
        } else
        {
            chatBox.ClearFor(player);
        }
    }

    [ScriptFunctionDefinition("outputChatBox")]
    public void OutputChatBox(string text, ElementTarget? visibleTo = null, int red = 231, int green = 217, int blue = 176, bool colorCoded = false)
    {
        var color = Color.FromArgb(red, green, blue);

        if (visibleTo?.Players != null)
        {
            foreach (var p in visibleTo.Players)
                chatBox.OutputTo(p, text, color, colorCoded);

            return;
        }

        if (visibleTo?.Element is Player player)
        {
            chatBox.OutputTo(player, text, color, colorCoded);
            return;
        }

        chatBox.Output(text, color, colorCoded);
    }

    [ScriptFunctionDefinition("showChat")]
    public bool ShowChat(Player thePlayer, bool show, bool? inputBlocked = null)
    {
        chatBox.SetVisibleFor(thePlayer, show, inputBlocked);
        return true;
    }

    [ScriptFunctionDefinition("outputConsole")]
    public bool OutputConsole(string text, Player? visibleTo = null)
    {
        if (visibleTo == null)
            clientConsole.Output(text);
        else
            clientConsole.OutputTo(visibleTo, text);
        return true;
    }
}
