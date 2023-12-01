namespace SlipeServer.LuaControllers.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CommandControllerAttribute(bool usesScopedEvents = true) : Attribute
{
    public bool UsesScopedCommands { get; } = usesScopedEvents;
}
