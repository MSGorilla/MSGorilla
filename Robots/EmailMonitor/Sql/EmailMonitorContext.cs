using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace MSGorilla.EmailMonitor.Sql
{
    public class EmailMonitorContext : DbContext
    {
        public DbSet<EmailMonitoringHistory> EmailMonitoringHistory { get; set; }
        public DbSet<Conversation> Conversation { get; set; }
    }
}
