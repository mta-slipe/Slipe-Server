namespace SlipeServer.Server.Elements;

public struct TimeContext
{
    private readonly object @lock;
    private byte current;

    public TimeContext()
    {
        this.current = 1;
        this.@lock = new();
    }

    /// <summary>
    /// The current time context value, used to verify whether synchronization packets are to be applied or ignored.
    /// </summary>
    public byte Current
    {
        get
        {
            lock (this.@lock)
            {
                return this.current;
            }
        }
    }

    /// <summary>
    /// Generates and returns a new time context. This can be used to invalidate synchronization updates sent prior to this moment.
    /// </summary>
    /// <returns>The new time context value.</returns>
    public byte GetAndIncrement()
    {
        lock (this.@lock)
        {
            if (++this.current == 0)
            {
                this.current++;
            }
            return this.current;
        }
    }

    public static implicit operator byte(TimeContext manager)
    {
        return manager.Current;
    }
}
