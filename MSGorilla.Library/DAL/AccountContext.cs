using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace MSGorilla.Library.Models.SqlModels
{
    public class AccountContext : DbContext
    {
        public DbSet<UserProfile> Users { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        public AccountContext()
        {
            base.Configuration.ProxyCreationEnabled = false;
        }
    }
}