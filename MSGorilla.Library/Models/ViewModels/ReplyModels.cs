using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public class ReplyPagination
    {
        public List<Reply> message { get; set; }
        public string continuationToken { get; set; }
    }

    public class DisplayReplyPagination
    {
        public string continuationToken { get; set; }
        public List<DisplayReply> message { get; set; }

        public DisplayReplyPagination(ReplyPagination rpl)
        {
            continuationToken = rpl.continuationToken;
            var replylist = rpl.message;
            AccountManager accManager = new AccountManager();
            AttachmentManager attManager = new AttachmentManager();
            message = new List<DisplayReply>();
            foreach (var r in replylist)
            {
                message.Add(new DisplayReply(r, accManager, attManager));
            }
        }
    }

    public class DisplayReply : DisplayMessage
    {
        public string Type
        {
            get
            {
                return "reply";
            }
        }
        public string MessageUser { get; set; }
        public string MessageID { get; set; }

        public DisplayReply(Reply rpl, AccountManager accManager, AttachmentManager attManager)
            : base(rpl, accManager, attManager)
        {
            this.MessageID = rpl.MessageID;
            this.MessageUser = rpl.MessageUser;
        }
    }
}
