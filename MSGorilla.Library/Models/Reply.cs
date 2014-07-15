using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MSGorilla.Library.Models
{
    public class Reply : Message
    {
        public string MessageUser { get; set; }
        public string MessageID { get; set; }
        public Reply(
                        string userid,
                        string message,
                        DateTime timestamp,
                        string messageUser,
                        string messageID,
                        string[] atUser = null,
                        string richMessageID = null,
                        string[] attachmentID = null,
                        string id = null
                ) 
            : base(userid, message, timestamp, "none", "none", null, atUser, null, richMessageID, attachmentID, 2, id)
        {
            MessageUser = messageUser;
            MessageID = messageID;           
        }
        public string toJsonString(){
            return JsonConvert.SerializeObject(this);
        }
    }
}
