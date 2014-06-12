using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Queue;

using MSGorilla.Library.Exceptions;

namespace MSGorilla.Library.Models
{
    public class Message
    {
        public string User { get; set; }
        public string ID { get; set; }
        public string EventID { get; set; }
        public string SchemaID { get; set; }
        public string[] Owner { get; set; }        
        public string[] AtUser { get; set; }
        public string[] TopicName { get; set; }
        public string MessageContent { get; set; } 
        public DateTime PostTime { get; set; }

        public Message(string userid, 
            string message, 
            DateTime timestamp, 
            string eventID, 
            string schemaID, 
            string[] owner,
            string[] atUser,
            string[] topicName)
        {
            User = userid;
            MessageContent = message;
            EventID = eventID;
            SchemaID = schemaID;
            Owner = owner;
            AtUser = atUser;
            TopicName = topicName;
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
            try
            {
                double timespan = Double.Parse(messageID.Split('_')[0]);
                DateTime timestamp = DateTime.MaxValue.AddMilliseconds(0 - timespan);

                return string.Format("{0}_{1}", userid, Utils.ToAzureStorageDayBasedString(timestamp, false));
            }
            catch
            {
                throw new InvalidMessageIDException();
            }
        }

        public CloudQueueMessage toAzureCloudQueueMessage()
        {
            return new CloudQueueMessage(JsonConvert.SerializeObject(this));
        }
    }
}
