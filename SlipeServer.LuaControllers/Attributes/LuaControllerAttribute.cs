namespace SlipeServer.LuaControllers.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class LuaControllerAttribute : Attribute
{
    public string EventPrefix { get; }
    public bool UsesScopedEvents { get; }

    public LuaControllerAttribute(string eventPrefix, bool usesScopedEvents = true)
    {
        this.EventPrefix = eventPrefix;
        this.UsesScopedEvents = usesScopedEvents;
    }
}
