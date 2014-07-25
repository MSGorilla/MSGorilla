using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Queue;

using MSGorilla.Library.Exceptions;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace MSGorilla.Library.Models
{
    public class Message
    {
        public string User { get; set; }
        public string ID { get; set; }
        public string Group { get; set; }
        public string EventID { get; set; }
        public string SchemaID { get; set; }
        public string[] Owner { get; set; }
        public string[] AtUser { get; set; }
        public string[] TopicName { get; set; }
        public string MessageContent { get; set; }
        public DateTime PostTime { get; set; }
        public string RichMessageID { get; set; }
        public string[] AttachmentID { get; set; }
        public int Importance { get; set; }

        public Message() { }
        public Message(string userid,
                        string groupID,
                        string message,
                        DateTime timestamp,
                        string eventID,
                        string schemaID,
                        string[] owner,
                        string[] atUser,
                        string[] topicName,
                        string richMessageID,
                        string[] attachmentID,
                        int importance = 2,
                        string msgID = null            
                )
        {
            User = userid;
            Group = groupID;
            MessageContent = message;
            EventID = eventID;
            SchemaID = schemaID;
            Owner = owner;
            AtUser = atUser;
            TopicName = topicName;
            RichMessageID = richMessageID;
            PostTime = timestamp.ToUniversalTime();
            AttachmentID = attachmentID;

            if (string.IsNullOrEmpty(msgID))
            {
                ID = string.Format("{0}_{1}_{2}_{3}",
                                Utils.ToAzureStorageSecondBasedString(PostTime),
                                Group,
                                User,
                                Guid.NewGuid().ToString());
            }
            else
            {
                ID = msgID;
            }

            if (importance < 0)
            {
                this.Importance = 0;
            }
            else
            {
                this.Importance = importance;
            }
        }

        public string ToJsonString()
        {
            //JavaScriptSerializer serialize = new JavaScriptSerializer();
            //return serialize.Serialize(this);
            return JsonConvert.SerializeObject(this);
        }

        public CloudQueueMessage toAzureCloudQueueMessage()
        {
            return new CloudQueueMessage(JsonConvert.SerializeObject(this));
        }
    }
}
