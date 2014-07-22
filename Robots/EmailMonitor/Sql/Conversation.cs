using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MSGorilla.EmailMonitor.Sql
{
    public class Conversation
    {
        [Key]
        public string ConversationID { get; set; }
        public string MessageUser { get; set; }
        public string MessageID { get; set; }
    }
}
