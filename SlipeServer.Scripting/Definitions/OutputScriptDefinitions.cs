using Microsoft.Extensions.Logging;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using System.Drawing;

namespace SlipeServer.Scripting.Definitions;

/// <summary>
/// https://wiki.multitheftauto.com/wiki/Server_Scripting_Functions#Output_functions
/// </summary>
public class OutputScriptDefinitions(IDebugLog debugLog, ILogger logger, IChatBox chatBox)
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
    public void OutputChatBox(string text, Player? player = null, int red = 231, int green = 217, int blue = 176, bool colorCoded = false)
    {
        var color = Color.FromArgb(red, green, blue);
        if (player == null)
        {
            chatBox.Output(text, color, colorCoded);
        } else
        {
            chatBox.OutputTo(player, text, color, colorCoded);
        }
    }
}
