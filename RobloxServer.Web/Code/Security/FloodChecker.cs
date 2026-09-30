using System;
using System.Web;
using System.Web.Caching;

namespace RobloxServer.Security
{
    /// <summary>
    /// Fixed-window counters kept in the ASP.NET cache. Used for the per-IP request limit
    /// (the old express-rate-limit: 45 requests / 30 seconds) and for login attempts.
    /// </summary>
    public static class FloodChecker
    {
        class Counter
        {
            public int Count;
        }

        static readonly object Sync = new object();

        /// <summary>Counts one hit and returns true when <paramref name="limit"/> has been exceeded.</summary>
        public static bool Hit(string key, int limit, TimeSpan window)
        {
            return Increment(key, window) > limit;
        }

        /// <summary>Returns true when the key is over the limit, without counting.</summary>
        public static bool IsFlooded(string key, int limit)
        {
            var counter = HttpRuntime.Cache["flood:" + key] as Counter;
            return counter != null && counter.Count >= limit;
        }

        public static int Increment(string key, TimeSpan window)
        {
            lock (Sync)
            {
                string cacheKey = "flood:" + key;
                var counter = HttpRuntime.Cache[cacheKey] as Counter;
                if (counter == null)
                {
                    counter = new Counter();
                    HttpRuntime.Cache.Insert(cacheKey, counter, null, DateTime.UtcNow.Add(window), Cache.NoSlidingExpiration);
                }
                counter.Count++;
                return counter.Count;
            }
        }

        public static void Reset(string key)
        {
            HttpRuntime.Cache.Remove("flood:" + key);
        }
    }
}
