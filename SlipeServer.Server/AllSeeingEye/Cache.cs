using System;

namespace SlipeServer.Server.AllSeeingEye;

/// <summary>
/// Cache to handle ASE query responses
/// </summary>
/// <typeparam name="T"></typeparam>
public class Cache<T>(Func<T> updateFunc, int cachePeriod)
{
    private T? Data { get; set; } = default;
    private long CacheTime { get; set; } = 0;
    private int CachePeriod { get; } = cachePeriod * 10000;

    private void UpdateCache()
    {
        this.Data = updateFunc();
        this.CacheTime = DateTime.Now.Ticks;
    }

    public T? Get(bool forceUpdate = false)
    {
#if DEBUG
        UpdateCache();
#endif
        if (forceUpdate || this.CacheTime + this.CachePeriod < DateTime.Now.Ticks)
            UpdateCache();
        return this.Data;
    }
}
