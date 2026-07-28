namespace SlipeServer.LuaControllers.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RateLimitAttribute : Attribute
{
    public TimeSpan TimeSpan { get; }

    public RateLimitAttribute(int intervalInMilliseconds)
    {
        this.TimeSpan = TimeSpan.FromMilliseconds(intervalInMilliseconds);
    }

    public RateLimitAttribute(int hours, int minutes, int seconds)
    {
        this.TimeSpan = new TimeSpan(hours, minutes, seconds);
    }

    public RateLimitAttribute(string timespan)
    {
        this.TimeSpan = TimeSpan.Parse(timespan);
    }
}
