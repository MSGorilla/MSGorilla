using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MSGorilla.Library.Models
{
    public class Message
    {
        public string User { get; set; }
        public string ID { get; set; }
        public string EventID { get; set; }
        public string SchemaID { get; set; }
        public string MessageContent { get; set; } 
        public DateTime PostTime { get; set; }

        public Message(string userid, string message, DateTime timestamp, string eventID, string schemaID)
        {
            User = userid;
            MessageContent = message;
            EventID = eventID;
            SchemaID = schemaID;
            PostTime = timestamp.ToUniversalTime();
            ID = string.Format("{0}_{1}",
                                Utils.ToAzureStorageSecondBasedString(PostTime),
                                Guid.NewGuid().ToString());
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static string ToMessagePK(string userid, string messageID)
        {
            return string.Format("{0}_{1}", userid, messageID.Substring(0, 8));
        }
    }
}
