using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using AdminPortal.Models;

namespace AdminPortal.DAL
{
    public class RoleClaimContext : DbContext
    {
        public RoleClaimContext() : base("RoleClaimContext") { }

        public DbSet<Task> Tasks { get; set; }
    }
}