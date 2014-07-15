using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Data.Entity.ModelConfiguration.Conventions;
using MSGorilla.Library.DAL;
namespace MSGorilla.StatusReporter
{
    public class HistoryDataContext : MSGorillaContext
    {
        public DbSet<HistoryData> HistoryMonitoringData { get; set; }

    }
}
