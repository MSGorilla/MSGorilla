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
            message = new List<DisplayMessage>();
            foreach (var m in msglist)
            {
                message.Add(new DisplayMessage(m, accManager));
            }
        }
    }

    public class DisplayMessage
    {
        public SimpleUserProfile User { get; set; }
        public string ID { get; set; }
        public string EventID { get; set; }
        public string SchemaID { get; set; }
        public string[] Owner { get; set; }
        public string MessageContent { get; set; }
        public DateTime PostTime { get; set; }

        public DisplayMessage(Message msg, AccountManager accManager)
        {
            //
            var userinfo = accManager.FindUser(msg.User);
            this.User = new SimpleUserProfile(userinfo);

            // use old id
            this.ID = msg.ID;
            this.EventID = msg.EventID;
            this.SchemaID = msg.SchemaID;
            this.Owner = msg.Owner;
            this.MessageContent = msg.MessageContent;
            this.PostTime = msg.PostTime;
        }
    }
}
