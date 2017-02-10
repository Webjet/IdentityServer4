using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdminPortal.Models
{
    #if INCLUDE_NOT_COVERED_BY_TESTS
    //"Not included in solution as its reference has been removed, it required database caching. Also not tested yet.
    // Might consider to include in a future"
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
           
        {
        }

        public DbSet<UserTokenCache> UserTokenCacheList { get; set; }
    }

    //public class ApplicationDbContext : DbContext
    //{
    //    public ApplicationDbContext()
    //        : base("DefaultConnection")
    //    {
    //    }

    //    public DbSet<UserTokenCache> UserTokenCacheList { get; set; }
    //}

    public class UserTokenCache
    {
        [Key]
        public int UserTokenCacheId { get; set; }
        public string webUserUniqueId { get; set; }
        public byte[] cacheBits { get; set; }
        public DateTime LastWrite { get; set; }
    }
#endif
}
