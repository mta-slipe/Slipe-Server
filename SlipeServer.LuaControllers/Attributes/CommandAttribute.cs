namespace SlipeServer.LuaControllers.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CommandAttribute(string command) : Attribute
{
    public string Command { get; } = command;
}
