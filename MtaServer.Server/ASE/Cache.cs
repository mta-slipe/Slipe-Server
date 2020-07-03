using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.ASE
{
    public class Cache<T>
    {
        private T Data { get; set; }
        private long CacheTime { get; set; } = 0;
        private int CachePeriod { get; } = 0;
        private Func<T> UpdateFunc { get; }

        private void UpdateCache()
        {
            Data = UpdateFunc();
            CacheTime = DateTime.Now.Ticks;
            Console.WriteLine("Update cache...");
        }

        public Cache(Func<T> updateFunc, int cachePeriod)
        {
            this.UpdateFunc = updateFunc;
            this.CachePeriod = cachePeriod * 10000;
        }

        public T Get(bool forceUpdate = false)
        {
            if (forceUpdate || CacheTime + CachePeriod < DateTime.Now.Ticks)
                UpdateCache();
            Console.WriteLine("Get Data");
            return Data;
        }
    }
}
