using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public class ReplyPagination
    {
        public List<Reply> reply { get; set; }
        public string continuationToken { get; set; }
    }

    public class DisplayReplyPagination
    {
        public string continuationToken { get; set; }
        public List<DisplayReply> reply { get; set; }

        public DisplayReplyPagination(ReplyPagination rpl)
        {
            continuationToken = rpl.continuationToken;
            var replylist = rpl.reply;
            AccountManager accManager = new AccountManager();
            AttachmentManager attManager = new AttachmentManager();
            reply = new List<DisplayReply>();
            foreach (var r in replylist)
            {
                reply.Add(new DisplayReply(r, accManager, attManager));
            }
        }
    }

    public class DisplayReply : DisplayMessage
    {
        public string type
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
