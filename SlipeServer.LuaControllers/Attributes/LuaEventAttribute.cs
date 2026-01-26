namespace SlipeServer.LuaControllers.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class LuaEventAttribute(string eventName) : Attribute
{
    public string EventName { get; } = eventName;
}
