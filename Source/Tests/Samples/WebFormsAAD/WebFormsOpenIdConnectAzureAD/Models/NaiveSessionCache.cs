using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace WebFormsOpenIdConnectAzureAD
{
    //from https://github.com/Azure-Samples/active-directory-dotnet-webapp-webapi-openidconnect/blob/master/TodoListWebApp/Utils/NaiveSessionCache.cs
    public class NaiveSessionCache : TokenCache
    {
        private static ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        string UserObjectId = string.Empty;
        string CacheId = string.Empty;
        private HttpContext _httpContext = null;

        public NaiveSessionCache(HttpContext httpContext, string userId)
        {
            UserObjectId = userId;
            _httpContext = httpContext;
            CacheId = UserObjectId + "_TokenCache";

            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            Load();
        }

        public void Load()
        {
            if (_httpContext.Session != null)
            {
                SessionLock.EnterReadLock();
                this.Deserialize((byte[])_httpContext.Session[CacheId]);
                SessionLock.ExitReadLock();
            }
        }

        public void Persist()
        {
            if (_httpContext.Session != null)
            {
                SessionLock.EnterWriteLock();

                // Optimistically set HasStateChanged to false. We need to do it early to avoid losing changes made by a concurrent thread.
                this.HasStateChanged = false;

                // Reflect changes in the persistent store
                _httpContext.Session[CacheId] = this.Serialize();
                SessionLock.ExitWriteLock();
            }
        }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();
            _httpContext.Session.Remove(CacheId);
        }

        // Triggered right before ADAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after ADAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (this.HasStateChanged)
            {
                Persist();
            }
        }
    }
}