namespace SlipeServer.LuaControllers.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class LuaControllerAttribute : Attribute
{
    public string EventPrefix { get; }

    public LuaControllerAttribute(string eventName)
    {
        this.EventPrefix = eventName;
    }
}
