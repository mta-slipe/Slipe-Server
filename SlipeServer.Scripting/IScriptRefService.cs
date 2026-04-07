using SlipeServer.Packets.Definitions.Lua;

namespace SlipeServer.Scripting;

public interface IScriptRefService
{
    int Ref(LuaValue value);
    LuaValue Deref(int id);
}
