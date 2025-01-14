﻿using System;

namespace SlipeServer.Server.AllSeeingEye;

/// <summary>
/// Cache to handle ASE query responses
/// </summary>
/// <typeparam name="T"></typeparam>
public class Cache<T>
{
    private T? Data { get; set; }
    private long CacheTime { get; set; } = 0;
    private int CachePeriod { get; } = 0;
    private Func<T> UpdateFunc { get; }

    private void UpdateCache()
    {
        this.Data = this.UpdateFunc();
        this.CacheTime = DateTime.Now.Ticks;
    }

    public Cache(Func<T> updateFunc, int cachePeriod)
    {
        this.UpdateFunc = updateFunc;
        this.CachePeriod = cachePeriod * 10000;
        this.Data = default;
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
