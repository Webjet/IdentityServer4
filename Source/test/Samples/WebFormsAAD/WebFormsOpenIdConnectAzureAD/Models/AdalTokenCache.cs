using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Security;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace WebFormsOpenIdConnectAzureAD.Models
{
    //TODO: change to Naive Cache Temporary
    public class ADALTokenCache : TokenCache
    {
        private bool _useEncryption;
        private ApplicationDbContext db = new ApplicationDbContext();
        private string _uniqueKey;
        private UserTokenCache Cache;

        public ADALTokenCache(string signedInUserId)
        {
            // associate the cache to the current user of the web app
            _uniqueKey = UserIdUniqueKey(signedInUserId);
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            this.BeforeWrite = BeforeWriteNotification;
            // look up the entry in the database
            Cache = db.UserTokenCacheList.FirstOrDefault(c => c.webUserUniqueId == _uniqueKey);
            // place the entry in memory
            this.Deserialize(GetState());
        }

        private static string UserIdUniqueKey(string signedInUserId)
        {
           //return HttpContext.Current.Server.MachineName +signedInUserId;
			return signedInUserId;
        }

        // clean up the database
        public override void Clear()
        {
            base.Clear();
            var cacheEntry = db.UserTokenCacheList.FirstOrDefault(c => c.webUserUniqueId == _uniqueKey);
            db.UserTokenCacheList.Remove(cacheEntry);
            db.SaveChanges();
        }

        // Notification raised before ADAL accesses the cache.
        // This is your chance to update the in-memory copy from the DB, if the in-memory version is stale
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            if (Cache == null)
            {
                // first time access
                Cache = db.UserTokenCacheList.FirstOrDefault(c => c.webUserUniqueId == _uniqueKey);
            }
            else
            { 
                // retrieve last write from the DB
                var status = from e in db.UserTokenCacheList
                             where (e.webUserUniqueId == _uniqueKey)
                select new
                {
                    LastWrite = e.LastWrite
                };

                // if the in-memory copy is older than the persistent copy
                if (status.First().LastWrite > Cache.LastWrite)
                {
                    // read from from storage, update in-memory copy
                    Cache = db.UserTokenCacheList.FirstOrDefault(c => c.webUserUniqueId == _uniqueKey);
                }
            }
            this.Deserialize(GetState());
        }

        private byte[] GetState()
        {
            if (Cache == null) return null;
            var bytes = Cache.cacheBits;
            if (_useEncryption)
            {
                bytes = MachineKey.Unprotect(bytes, "ADALCache");
            }
            return  bytes;
        }

        // Notification raised after ADAL accessed the cache.
        // If the HasStateChanged flag is set, ADAL changed the content of the cache
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if state changed
            if (this.HasStateChanged)
            {
                var userData = this.Serialize();
                Cache = new UserTokenCache
                {
                    webUserUniqueId = _uniqueKey,
                    cacheBits = SetState(userData),
                    LastWrite = DateTime.Now
                };
                // update the DB and the lastwrite 
                db.Entry(Cache).State = Cache.UserTokenCacheId == 0 ? EntityState.Added : EntityState.Modified;
                db.SaveChanges();
                this.HasStateChanged = false;
            }
        }

        private byte[] SetState(byte[] userData)
        {

            if (_useEncryption)
                return MachineKey.Protect(userData, "ADALCache");
            else
                return userData;
        }

        void BeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        }

        public override void DeleteItem(TokenCacheItem item)
        {
            base.DeleteItem(item);
        }
    }
}
