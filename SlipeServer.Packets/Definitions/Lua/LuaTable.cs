using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SlipeServer.Packets.Definitions.Lua;

[DebuggerTypeProxy(typeof(LuaTableDebuggingView))]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class LuaTable : Dictionary<LuaValue, LuaValue>
{
    public bool IsSequential => LuaValue.IsSequentialTableValue(this);

    public LuaTable(params LuaValue[] luaValues)
    {
        int i = 0;
        foreach (var luaValue in luaValues)
        {
            this[++i] = luaValue;
        }
    }

    private string DebuggerDisplay => this.IsSequential ? $"Sequential table of Length={this.Count}" : $"Associative table of Length={this.Count}";
}
