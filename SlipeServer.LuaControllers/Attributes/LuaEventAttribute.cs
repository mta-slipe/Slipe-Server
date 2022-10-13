namespace SlipeServer.LuaControllers.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class LuaEventAttribute : Attribute
{
    public string EventName { get; }

    public LuaEventAttribute(string eventName)
    {
        this.EventName = eventName;
    }
}
