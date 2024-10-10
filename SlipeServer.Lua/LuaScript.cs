using MoonSharp.Interpreter;
using SlipeServer.Scripting;

namespace SlipeServer.Lua;

internal class LuaScript : IScript
{
    private readonly Script script;
    public string Language { get; } = "Lua";

    public LuaScript(Script script)
    {
        this.script = script;
    }

    public void LoadCode(byte[] code, string chunkName)
    {
        try
        {
            var source = System.Text.Encoding.UTF8.GetString(code);
            this.script.DoString(source, null, chunkName);
        }
        catch(ScriptRuntimeException ex)
        {
            throw new ScriptingException(ex.DecoratedMessage, ex);
        }
    }

    public object? GetGlobal(string name)
    {
        return this.script.Globals[name];
    }

    public void SetGlobal(string name, object? value)
    {
        if (value == null)
        {
            this.script.Globals.Remove(name);
        } else
        {
            this.script.Globals[name] = value;
        }
    }
}
