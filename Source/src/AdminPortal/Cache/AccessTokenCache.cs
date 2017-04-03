using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminPortal.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;

namespace AdminPortal
{
    public class AccessTokenCache : ICacheService
    {
        //private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        //{

        //    return ConnectionMultiplexer.Connect("WebjetAdminPortalDev.redis.cache.windows.net:6380,password=A1knMDLBEGHZ7HDlvcW6bEVJqA0HdwBTZrJs4d5is5k=,ssl=True,abortConnect=False");
        //    //return ConnectionMultiplexer.Connect("WebjetAdminPortalDev.redis.cache.windows.net:6380,password=A1knMDLBEGHZ7HDlvcW6bEVJqA0HdwBTZrJs4d5is5k=,ssl=True,abortConnect=False");
        //});

        //public static ConnectionMultiplexer Connection
        //{
        //    get
        //    {
        //        return lazyConnection.Value;
        //    }
        //}

        protected IDistributedCache _cache;
        public const int DefaultCacheDuration = 60;
        public const string AccessTokenKey = "AADAccessToken";

        public AccessTokenCache(IDistributedCache cache)
        {
            _cache = cache;
        }


        public T Get<T>(string key) where T : class
        {
            var fromCache = _cache.Get(key);
            if (fromCache == null)
            {
                return null;
            }

            var str = Encoding.UTF8.GetString(fromCache);
            if (typeof(T) == typeof(string))
            {
                return str as T;
            }

            JsonSerializerSettings serSettings = new JsonSerializerSettings();
            serSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return JsonConvert.DeserializeObject<T>(str, serSettings);

        }

        public void Store(string key, object content)
        {
            Store(key, content, DefaultCacheDuration);
        }

        public void Store(string key, object content, int duration)
        {
            string toStore;
            if (content is string)
            {
                toStore = (string)content;
            }
            else
            {
                toStore = JsonConvert.SerializeObject(content);
            }

            duration = duration <= 0 ? DefaultCacheDuration : duration;
            _cache.Set(key, Encoding.UTF8.GetBytes(toStore), new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now + TimeSpan.FromSeconds(duration)
            });

        }

    }
}
