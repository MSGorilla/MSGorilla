using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class BaseMessageEntity : TableEntity
    {
        public string User { get; set; }
        public string ID { get; set; }
        public string EventID { get; set; }
        public string SchemaID { get; set; }
        public string Owner { get; set; }
        public string AtUser { get; set; }
        public string TopicName { get; set; }
        public string MessageContent { get; set; }
        public DateTime PostTime { get; set; }
        public string RichMessageID { get; set; }
        public string AttachmentID { get; set; }
        public string Content { get; set; }
        public int Importance { get; set; }

        public BaseMessageEntity() {}

        public BaseMessageEntity(Message message, string pk = null, string rk = null)
        {
            this.User = message.User;
            this.ID = message.ID;
            this.EventID = message.EventID;
            this.SchemaID = message.SchemaID;
            this.Owner = Utils.StringArray2String(message.Owner);
            this.AtUser = Utils.StringArray2String(message.AtUser);
            this.TopicName = Utils.StringArray2String(message.TopicName);
            this.MessageContent = message.MessageContent;
            this.PostTime = message.PostTime;
            this.RichMessageID = message.RichMessageID;
            this.AttachmentID = Utils.StringArray2String(message.AttachmentID);
            this.Importance = message.Importance;

            if (!string.IsNullOrEmpty(pk))
            {
                this.PartitionKey = pk;
            }
            if (!string.IsNullOrEmpty(rk))
            {
                this.RowKey = rk;
            }
        }

        public Message toMessage()
        {
            //compatible with old version one column format
            if (!string.IsNullOrEmpty(this.Content))
            {
                return JsonConvert.DeserializeObject<Message>(this.Content);
            }

            Message message = new Message(
                    this.User,
                    this.MessageContent,
                    this.PostTime,
                    this.EventID,
                    this.SchemaID,
                    Utils.String2StringArray(this.Owner),
                    Utils.String2StringArray(this.AtUser),
                    Utils.String2StringArray(this.TopicName),
                    this.RichMessageID,
                    Utils.String2StringArray(this.AttachmentID),
                    this.Importance,
                    this.ID                    
                );
            return message;
        }
    }
}
