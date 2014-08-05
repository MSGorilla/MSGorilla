using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MSGorilla.Utility
{
    public class CacheHelper
    {
        public const string MembershipPrefix = "membership#";
        public const string JoinedGroupPrefix = "joinedGroup#";
        public const string SimpleUserprofilePrefix = "simpleUserprofile#";
        public const string CategoryPrefix = "category#";


        private const int DefaultCacheTimeSpanMin = 15;
        public static void Add<T>(string key, T value, DateTime? absoluteExpiration = null) where T : class
        {
            if (absoluteExpiration == null)
            {
                absoluteExpiration = DateTime.Now.AddMinutes(DefaultCacheTimeSpanMin);
            }
            HttpContext.Current.Cache.Insert(
                    key,
                    value,
                    null,
                    absoluteExpiration.Value,
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