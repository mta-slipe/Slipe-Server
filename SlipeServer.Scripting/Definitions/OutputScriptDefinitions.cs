using Microsoft.Extensions.Logging;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using System.Drawing;

namespace SlipeServer.Scripting.Definitions;

/// <summary>
/// https://wiki.multitheftauto.com/wiki/Server_Scripting_Functions#Output_functions
/// </summary>
public class OutputScriptDefinitions
{
    private readonly DebugLog debugLog;
    private readonly ILogger logger;
    private readonly ChatBox chatBox;

    public OutputScriptDefinitions(DebugLog debugLog, ILogger logger, ChatBox chatBox)
    {
        this.debugLog = debugLog;
        this.logger = logger;
        this.chatBox = chatBox;
    }

    [ScriptFunctionDefinition("outputDebugString")]
    public void OutputDebugString(string message, DebugLevel level = DebugLevel.Information, int red = 255, int green = 255, int blue = 255)
    {
        var color = Color.FromArgb(red, green, blue);
        this.debugLog.Output(message, level, color);
        switch (level)
        {
            case DebugLevel.Information:
                this.logger.LogInformation(message);
                break;
            case DebugLevel.Warning:
                this.logger.LogWarning(message);
                break;
            case DebugLevel.Error:
                this.logger.LogError(message);
                break;
            case DebugLevel.Custom:
            default:
                this.logger.LogDebug(message);
                break;
        }
    }

    [ScriptFunctionDefinition("outputServerLog")]
    public void OutputServerLog(string message)
    {
        this.logger.LogDebug(message);
    }

    [ScriptFunctionDefinition("clearChatBox")]
    public void ClearChatBox(Player? player = null)
    {
        if(player == null)
        {
            this.chatBox.Clear();
        } else
        {
            this.chatBox.ClearFor(player);
        }
    }

    [ScriptFunctionDefinition("outputChatBox")]
    public void OutputChatBox(string text, Player? player = null, int red = 231, int green = 217, int blue = 176, bool colorCoded = false)
    {
        var color = Color.FromArgb(red, green, blue);
        if (player == null)
        {
            this.chatBox.Output(text, color, colorCoded);
        } else
        {
            this.chatBox.OutputTo(player, text, color, colorCoded);
        }
    }
}
