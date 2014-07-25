using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MSGorilla.Utility
{
    public class CacheHelper
    {
        public static int cacheTimeSpanMin = 10;
        public static void Add<T>(string key, T value) where T : class
        {
            HttpContext.Current.Cache.Insert(
                    key,
                    value,
                    null,
                    DateTime.Now.AddMinutes(cacheTimeSpanMin),
                    System.Web.Caching.Cache.NoSlidingExpiration
                );
        }

        public static T Get<T>(string key) where T : class
        {
            return (T)HttpContext.Current.Cache.Get(key);
        }

        public static void Remove(string key)
        {
            HttpContext.Current.Cache.Remove(key);
        }

        public static bool Contains(string key)
        {
            return HttpContext.Current.Cache[key] != null;
        }
    }
}