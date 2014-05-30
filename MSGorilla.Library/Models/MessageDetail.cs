using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models
{
    public class MessageDetail
    {
        public string User { get; set; }
        public string ID { get; set; }
        public string EventID { get; set; }
        public string SchemaID { get; set; }
        public string TopicID { get; set; }
        public string[] Owner { get; set; }
        public string[] AtUser { get; set; }
        public string MessageContent { get; set; }
        public DateTime PostTime { get; set; }

        public int ReplyCount;
        public List<Reply> Replies;

        public MessageDetail(Message msg)
        {
            this.User = msg.User;
            this.ID = msg.ID;
            this.EventID = msg.EventID;
            this.SchemaID = msg.SchemaID;
            this.TopicID = msg.TopicID;
            this.Owner = msg.Owner;
            this.AtUser = msg.AtUser;
            this.MessageContent = msg.MessageContent;
            this.PostTime = msg.PostTime;
        }
    }
}
