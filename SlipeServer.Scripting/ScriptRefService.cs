using SlipeServer.Packets.Definitions.Lua;
using System.Collections.Generic;

namespace SlipeServer.Scripting;

public class ScriptRefService : IScriptRefService
{
    private readonly Dictionary<int, LuaValue> refs = [];
    private int nextId = 1;

    public int Ref(LuaValue value)
    {
        int id = this.nextId++;
        this.refs[id] = value;
        return id;
    }

    public LuaValue Deref(int id)
    {
        return this.refs.TryGetValue(id, out var value) ? value : LuaValue.Nil;
    }
}
