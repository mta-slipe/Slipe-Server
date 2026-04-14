using SlipeServer.LuaControllers;
using SlipeServer.LuaControllers.Attributes;
using SlipeServer.Server.Services;

namespace SlipeServer.DropInReplacement.BuiltInCommands;

[CommandController]
public class DebugCommandController(IChatBox chatbox, IDebugLog debugLog) : BaseCommandController
{
    [Command("debugscript")]
    public void SetDebugScriptLevel(int level)
    {
        if (level < 0 || level > 3)
        {
            chatbox.OutputTo(Context.Player, "Invalid debug level. Please provide a value between 0 and 3.");
            return;
        }

        this.Context.Player.DebugLogLevel = level;
        debugLog.SetVisible(level != 0);
    }
}
