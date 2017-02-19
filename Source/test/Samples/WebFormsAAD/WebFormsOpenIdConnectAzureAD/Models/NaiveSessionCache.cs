using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private bool _useSession=false;
        string UserObjectId = string.Empty;
        string CacheId = string.Empty;
        private HttpContext _httpContext = null;

        public NaiveSessionCache(HttpContext httpContext, string userId,bool useSession)
        {
            UserObjectId = userId;
            _httpContext = httpContext;
            _useSession = useSession;
            CacheId = UserObjectId + "_TokenCache";
            if (_useSession)
            {
                Debug.Assert(_httpContext.Session !=null);
            }
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            Load();
        }

        public void Load()
        {
            SessionLock.EnterReadLock();
            var state = GetStateFromSessionOrCache();
            this.Deserialize(state);
            SessionLock.ExitReadLock();
         
        }

        private byte[] GetStateFromSessionOrCache()
        {
            var httpContextSession = _httpContext.Session;
            byte[] state = null;
            if (_useSession)
            {
                state = (byte[]) httpContextSession[CacheId];
            }
            else
            {
                state = (byte[]) _httpContext.Cache[CacheId];
            }
            return state;
        }
        private void SetStateToSessionOrCache(byte[] state)
        {
            var httpContextSession = _httpContext.Session;
           
            if (_useSession)
            {
                httpContextSession[CacheId]= state;
            }
            else
            {
                  _httpContext.Cache[CacheId]= state;
            }
         }
        private void ClearSessionOrCache()
        {
            var httpContextSession = _httpContext.Session;

            if (_useSession)
            {
                httpContextSession.Remove(CacheId);
             }
            else
            {
                _httpContext.Cache.Remove(CacheId);
            }
        }
        public void Persist()
        {
                SessionLock.EnterWriteLock();

                // Optimistically set HasStateChanged to false. We need to do it early to avoid losing changes made by a concurrent thread.
                this.HasStateChanged = false;

                // Reflect changes in the persistent store
                //_httpContext.Session[CacheId] =
                SetStateToSessionOrCache(this.Serialize());
                SessionLock.ExitWriteLock();
         }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();
            // _httpContext.Session.Remove(CacheId);
            ClearSessionOrCache();
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