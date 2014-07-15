using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public class MessagePagination
    {
        public List<Message> message { get; set; }
        public string continuationToken { get; set; }
    }

    public class DisplayMessagePagination
    {
        public string continuationToken { get; set; }
        public List<DisplayMessage> message { get; set; }

        public DisplayMessagePagination(MessagePagination msg)
        {
            continuationToken = msg.continuationToken;
            var msglist = msg.message;
            AccountManager accManager = new AccountManager();
            AttachmentManager attManager = new AttachmentManager();
            message = new List<DisplayMessage>();
            foreach (var m in msglist)
            {
                message.Add(new DisplayMessage(m, accManager, attManager));
            }
        }

        public DisplayMessagePagination() { }
    }

    public class DisplayMessage
    {
        public string type
        {
            get
            {
                return "message";
            }
        }
        public SimpleUserProfile User { get; set; }
        public string ID { get; set; }
        public string EventID { get; set; }
        public string SchemaID { get; set; }
        public string[] Owner { get; set; }
        public string[] AtUser { get; set; }
        public string[] TopicName { get; set; }
        public string MessageContent { get; set; }
        public string RichMessageID { get; set; }
        public List<Attachment> Attachment { get; set; } 
        public DateTime PostTime { get; set; }
        public int Importance { get; set; }

        public DisplayMessage() { }

        public DisplayMessage(Message msg, AccountManager accManager, AttachmentManager attManager)
        {
            //
            var userinfo = accManager.FindUser(msg.User);
            this.User = new SimpleUserProfile(userinfo);

            // use old id
            this.ID = msg.ID;
            this.EventID = msg.EventID;
            this.SchemaID = msg.SchemaID;
            this.Owner = msg.Owner;
            this.AtUser = msg.AtUser;
            this.TopicName = msg.TopicName;
            this.MessageContent = msg.MessageContent;
            this.RichMessageID = msg.RichMessageID;
            this.PostTime = msg.PostTime;
            this.Importance = msg.Importance;

            if (msg.AttachmentID == null || msg.AttachmentID.Length == 0)
            {
                this.Attachment = null;
            }
            else
            {
                this.Attachment = new List<Attachment>();
                foreach (string attid in msg.AttachmentID)
                {
                    Attachment att = attManager.GetAttachmentInfo(attid);
                    if (att != null)
                    {
                        this.Attachment.Add(att);
                    }
                }
            }
        }
    }
}
