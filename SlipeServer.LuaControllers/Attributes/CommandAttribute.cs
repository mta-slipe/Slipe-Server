namespace SlipeServer.LuaControllers.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CommandAttribute(string command, bool isCaseSensitive = false) : Attribute
{
    public string Command { get; } = command;
    public bool IsCaseSensitive { get; } = isCaseSensitive;
}
