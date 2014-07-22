using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MSGorilla.EmailMonitor.Sql
{
    public class EmailMonitoringHistory
    {
        [Key]
        public string Address { get; set; }
        public DateTime LastUpdateTimestamp { get; set; }
    }
}
