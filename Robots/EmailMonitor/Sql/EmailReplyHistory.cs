using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MSGorilla.EmailMonitor.Sql
{
    public class EmailReplyHistory
    {
        [Key]
        public string Userid { get; set; }
        public DateTime LastReplyTimestamp { get; set; }
        public string LastReplyID { get; set; }
    }
}
