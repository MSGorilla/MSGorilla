using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Library.DAL
{
    public class MSGorillaContext : DbContext
    {
        public DbSet<UserProfile> Users { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Schema> Schemas { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<NotificationCount> NotifCounts { get; set; }
        public DbSet<FavouriteTopic> favouriteTopic { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        public MSGorillaContext()
        {
            base.Configuration.ProxyCreationEnabled = false;
        }
    }
}