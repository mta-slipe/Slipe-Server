namespace SlipeServer.LuaControllers.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class TimedAttribute : Attribute
{
    public TimeSpan Interval { get; }

    public TimedAttribute(int intervalInMilliseconds)
    {
        this.Interval = TimeSpan.FromMilliseconds(intervalInMilliseconds);
    }

    public TimedAttribute(int hours, int minutes, int seconds)
    {
        this.Interval = new TimeSpan(hours, minutes, seconds);
    }

    public TimedAttribute(string timespan)
    {
        this.Interval = TimeSpan.Parse(timespan);
    }
}
