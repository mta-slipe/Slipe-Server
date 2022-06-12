using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Events;
using SlipeServer.SourceGenerators;

namespace SlipeServer.Console.LuaValues;

[LuaValue]
public partial class UiActiveStateLuaValue : ILuaValue
{
    public bool IsChatBoxInputActive { get; set; }
    public bool IsConsoleActive { get; set; }
    public bool IsDebugViewActive { get; set; }
    public bool IsMainMenuActive { get; set; }
    public bool IsMTAWindowActive { get; set; }
    public bool IsTransferBoxActive { get; set; }

    public partial void Parse(LuaValue luaValue);
}
