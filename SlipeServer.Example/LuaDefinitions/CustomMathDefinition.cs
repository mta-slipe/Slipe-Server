using SlipeServer.Scripting;

namespace SlipeServer.Example.LuaDefinitions;

public class CustomMathDefinition
{
    [ScriptFunctionDefinition("add")]
    public int Add(int a, int b)
    {
        return a + b;
    }

    [ScriptFunctionDefinition("substract")]
    public int Substract(int a, int b)
    {
        return a - b;
    }
}
